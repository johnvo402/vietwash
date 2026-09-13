using System.Data.Common;
using System.Linq.Expressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Events.CreateEInvoiceEvents;
using Application.Features.Funds.Events;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.EInvoices;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace FinanceService.Tests;

public class EInvoiceIdempotencyTests
{
    [Fact]
    public async Task SamePayloadTwice_CreatesOneInvoiceAndAcknowledgesRetry()
    {
        var repository = new Mock<IAsyncRepository<EInvoice>>(MockBehavior.Strict);
        repository
            .SetupSequence(x =>
                x.AnyAsync(
                    It.IsAny<Expression<Func<EInvoice, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false)
            .ReturnsAsync(true);
        repository
            .Setup(x => x.AddAsync(It.IsAny<EInvoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EInvoice invoice, CancellationToken _) => invoice);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork
            .Setup(x => x.Repository<EInvoice>(It.IsAny<bool>()))
            .Returns(repository.Object);
        unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<DbTransaction>());
        unitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var qr = new Mock<IQrGenerator>(MockBehavior.Strict);
        qr.Setup(x => x.GenerateQrBase64(It.IsAny<string>())).Returns("qr");
        var request = ValidRequest();
        var handler = new CreateEInvoiceEventHandler(unitOfWork.Object, ValidOrg(), qr.Object);

        var first = await handler.Handle(request, CancellationToken.None);
        var retry = await handler.Handle(request, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(retry.IsSuccess);
        Assert.Equal(request.PayloadId, retry.PayloadId);
        repository.Verify(
            x => x.AddAsync(It.IsAny<EInvoice>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        qr.Verify(x => x.GenerateQrBase64(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExistingSourceEvent_IsAcknowledgedWithoutCreatingAnotherInvoice()
    {
        var repository = new Mock<IAsyncRepository<EInvoice>>(MockBehavior.Strict);
        repository
            .Setup(x =>
                x.AnyAsync(
                    It.IsAny<Expression<Func<EInvoice, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork
            .Setup(x => x.Repository<EInvoice>(It.IsAny<bool>()))
            .Returns(repository.Object);
        var qr = new Mock<IQrGenerator>(MockBehavior.Strict);
        var request = new CreateEInvoiceEvent
        {
            PayloadId = Guid.NewGuid(),
            Payload = new EInvoiceOrderMessage { OrderId = 1001 },
        };

        var result = await new CreateEInvoiceEventHandler(
            unitOfWork.Object,
            new OrgSetting(),
            qr.Object
        ).Handle(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.PayloadId, result.PayloadId);
        unitOfWork.VerifyAll();
        repository.VerifyAll();
        qr.VerifyNoOtherCalls();
    }

    [Fact]
    public void SourceEventId_HasFilteredUniqueIndex()
    {
        using var context = new TheDbContext(
            new DbContextOptionsBuilder<TheDbContext>()
                .UseNpgsql("Host=localhost;Database=model_only")
                .Options
        );
        var index = context
            .Model.FindEntityType(typeof(EInvoice))!
            .GetIndexes()
            .Single(x => x.Properties.Single().Name == nameof(EInvoice.SourceEventId));

        Assert.True(index.IsUnique);
        Assert.Equal("source_event_id IS NOT NULL", index.GetFilter());
    }

    [Theory]
    [InlineData(UpdateStatusOrderEventHandler.SourceEventIndexName, false)]
    [InlineData(CreateEInvoiceEventHandler.SourceEventIndexName, true)]
    public void DuplicateClassifier_RequiresEInvoiceSourceEventIndex(
        string constraint,
        bool expected
    )
    {
        var postgres = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation,
            "",
            "",
            0,
            0,
            "",
            "",
            "public",
            "e_invoice",
            "source_event_id",
            "uuid",
            constraint,
            "",
            "",
            ""
        );

        Assert.Equal(
            expected,
            CreateEInvoiceEventHandler.IsDuplicateSourceEvent(
                new DbUpdateException("duplicate", postgres)
            )
        );
    }

    private static CreateEInvoiceEvent ValidRequest() =>
        new()
        {
            PayloadId = Guid.NewGuid(),
            Payload = new EInvoiceOrderMessage
            {
                MessageId = Guid.NewGuid(),
                OrderId = 1001,
                OrderCode = "OD-1001",
                CompletedAt = DateTimeOffset.UtcNow,
                CustomerName = "Outbox customer",
                Total = 100_000,
                Vat = 8,
                VatAmount = 8_000,
                Items =
                [
                    new EInvoiceOrderItemMessage
                    {
                        ServiceName = "Wash",
                        Quantity = 1,
                        UnitPrice = 100_000,
                    },
                ],
            },
        };

    private static OrgSetting ValidOrg() =>
        new()
        {
            OrgName = "VietWash",
            OrgTaxCode = "0123456789",
            OrgAddress = "HCMC",
            OrgPhone = "0900000000",
            Logo = "logo",
            Stamp = "stamp",
        };
}
