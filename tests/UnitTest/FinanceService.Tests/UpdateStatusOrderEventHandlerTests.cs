using System.Data.Common;
using System.Linq.Expressions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Funds.Events;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using PointTransaction = Domain.Aggregates.Funds.Transaction;

namespace FinanceService.Tests;

public class UpdateStatusOrderEventHandlerTests
{
    [Fact]
    public async Task SamePayloadTwice_ReturnsSuccessAndPersistsOneFundAndOnePointEntry()
    {
        var harness = new HandlerHarness();
        var request = CreateRequest(100_000m);

        var first = await harness.Handle(request);
        var second = await harness.Handle(request);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Single(harness.Funds);
        Assert.Single(harness.Transactions);
        Assert.Equal(100m, PointLedgerBalance(harness));
    }

    [Fact]
    public async Task DifferentPayloadIds_PersistIndependentFinanceEvents()
    {
        var harness = new HandlerHarness();

        var first = await harness.Handle(CreateRequest(100_000m));
        var second = await harness.Handle(CreateRequest(100_000m));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, harness.Funds.Count);
        Assert.Equal(2, harness.Transactions.Count);
        Assert.Equal(200m, PointLedgerBalance(harness));
    }

    [Fact]
    public async Task DuplicateDetectedByDatabaseRace_IsAcknowledgedWithoutDuplicates()
    {
        var harness = new HandlerHarness();
        var request = CreateRequest(100_000m);
        Assert.True((await harness.Handle(request)).IsSuccess);
        harness.BypassReadOptimization = true;

        var replay = await harness.Handle(request);

        Assert.True(replay.IsSuccess);
        Assert.Single(harness.Funds);
        Assert.Single(harness.Transactions);
        Assert.Equal(1, harness.RollbackCount);
    }

    [Fact]
    public async Task ZeroAmount_PersistsFundWithoutPointTransaction()
    {
        var harness = new HandlerHarness();

        var result = await harness.Handle(CreateRequest(0m));

        Assert.True(result.IsSuccess);
        Assert.Single(harness.Funds);
        Assert.Empty(harness.Transactions);
    }

    [Fact]
    public async Task GuestOrder_PersistsFundWithoutPointTransaction()
    {
        var harness = new HandlerHarness();
        var request = CreateRequest(100_000m);
        request.Payload!.ObjectId = null;

        var result = await harness.Handle(request);

        Assert.True(result.IsSuccess);
        Assert.Single(harness.Funds);
        Assert.Empty(harness.Transactions);
    }

    [Fact]
    public async Task NegativeAmount_IsPersistentFailureAndChangesNothing()
    {
        var harness = new HandlerHarness();

        var result = await harness.Handle(CreateRequest(-1m));

        Assert.False(result.IsSuccess);
        Assert.Equal(PubSubErrorType.Persistent, result.ErrorType);
        Assert.Empty(harness.Funds);
        Assert.Empty(harness.Transactions);
    }

    [Fact]
    public async Task SaveFailure_RollsBackFundAndPointsTogether()
    {
        var harness = new HandlerHarness { FailSave = true };

        var result = await harness.Handle(CreateRequest(100_000m));

        Assert.False(result.IsSuccess);
        Assert.Equal(PubSubErrorType.Transient, result.ErrorType);
        Assert.Empty(harness.Funds);
        Assert.Empty(harness.Transactions);
        Assert.Equal(1, harness.RollbackCount);
    }

    [Fact]
    public void DuplicateClassifier_RejectsUnrelatedUniqueViolation()
    {
        var exception = DuplicateException("some_other_unique_index");

        Assert.False(UpdateStatusOrderEventHandler.IsDuplicateSourceEvent(exception));
    }

    private static UpdateStatusOrderEvent CreateRequest(decimal amount) =>
        new()
        {
            PayloadId = Guid.NewGuid(),
            Payload = PointIntegrityTests.CreatePayload(amount),
        };

    private static decimal PointLedgerBalance(HandlerHarness harness) =>
        harness.Transactions.Sum(transaction => transaction.Amount);

    private static DbUpdateException DuplicateException(string indexName)
    {
        var postgresException = new PostgresException(
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
            "fund",
            "source_event_id",
            "uuid",
            indexName,
            "",
            "",
            ""
        );
        return new DbUpdateException("Duplicate finance event.", postgresException);
    }

    private sealed class HandlerHarness
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IAsyncRepository<Fund>> _fundRepository = new();
        private readonly Mock<IAsyncRepository<PointTransaction>> _transactionRepository = new();
        private Fund? _stagedFund;
        private readonly List<PointTransaction> _stagedTransactions = [];

        public List<Fund> Funds { get; } = [];
        public List<PointTransaction> Transactions { get; } = [];
        public bool BypassReadOptimization { get; set; }
        public bool FailSave { get; set; }
        public int RollbackCount { get; private set; }

        public HandlerHarness()
        {
            _unitOfWork
                .Setup(unit => unit.Repository<Fund>(It.IsAny<bool>()))
                .Returns(_fundRepository.Object);
            _unitOfWork
                .Setup(unit => unit.Repository<PointTransaction>(It.IsAny<bool>()))
                .Returns(_transactionRepository.Object);

            _fundRepository
                .Setup(repository =>
                    repository.AnyAsync(
                        It.IsAny<Expression<Func<Fund, bool>>>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(
                    (
                        Expression<Func<Fund, bool>> criteria,
                        CancellationToken _
                    ) =>
                        Task.FromResult(
                            !BypassReadOptimization && Funds.Any(criteria.Compile())
                        )
                );
            _fundRepository
                .Setup(repository =>
                    repository.AddAsync(It.IsAny<Fund>(), It.IsAny<CancellationToken>())
                )
                .Returns(
                    (Fund fund, CancellationToken _) =>
                    {
                        _stagedFund = fund;
                        return Task.FromResult(fund);
                    }
                );
            _transactionRepository
                .Setup(repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<PointTransaction>>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(
                    (IEnumerable<PointTransaction> entries, CancellationToken _) =>
                    {
                        _stagedTransactions.AddRange(entries);
                        return Task.FromResult(entries);
                    }
                );

            _unitOfWork
                .Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((DbTransaction)null!);
            _unitOfWork
                .Setup(unit => unit.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        if (FailSave)
                            throw new DbUpdateException("Simulated save failure.");

                        if (
                            _stagedFund?.SourceEventId is Guid sourceEventId
                            && Funds.Any(fund => fund.SourceEventId == sourceEventId)
                        )
                            throw DuplicateException(
                                UpdateStatusOrderEventHandler.SourceEventIndexName
                            );

                        return Task.CompletedTask;
                    }
                );
            _unitOfWork
                .Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        if (_stagedFund is not null)
                            Funds.Add(_stagedFund);
                        Transactions.AddRange(_stagedTransactions);
                        ClearStaged();
                        return Task.CompletedTask;
                    }
                );
            _unitOfWork
                .Setup(unit => unit.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken _) =>
                    {
                        RollbackCount++;
                        ClearStaged();
                        return Task.CompletedTask;
                    }
                );
        }

        public async Task<PubSubResponse<UpdateStatusOrderEvent>> Handle(
            UpdateStatusOrderEvent request
        )
        {
            var handler = new UpdateStatusOrderEventHandler(_unitOfWork.Object);
            return await handler.Handle(request, CancellationToken.None);
        }

        private void ClearStaged()
        {
            _stagedFund = null;
            _stagedTransactions.Clear();
        }
    }
}
