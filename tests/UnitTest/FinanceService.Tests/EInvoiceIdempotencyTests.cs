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
}
