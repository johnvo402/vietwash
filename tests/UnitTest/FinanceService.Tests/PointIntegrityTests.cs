using Application.Features.Common.Enums;
using Application.Features.Funds.Events;
using Application.Features.Transactions;
using Domain.Aggregates.Funds.Enums;

namespace FinanceService.Tests;

public class PointIntegrityTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1000, 1)]
    [InlineData(1001, 2)]
    [InlineData(1999, 2)]
    [InlineData(2000, 2)]
    public void CalculateEarnedPoints_UsesExistingCeilingRule(decimal amount, decimal expected)
    {
        Assert.Equal(expected, PointCalculator.CalculateEarnedPoints(amount));
    }

    [Fact]
    public void CalculateEarnedPoints_RejectsNegativeAmount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PointCalculator.CalculateEarnedPoints(-1m)
        );
    }

    [Fact]
    public void CalculateBalance_EmptyLedger_ReturnsZero()
    {
        Assert.Equal(0m, PointLedger.CalculateBalance([]));
    }

    [Fact]
    public void CalculateBalance_EarnAndSpend_ReturnsLedgerSum()
    {
        Assert.Equal(75m, PointLedger.CalculateBalance([100m, -40m, 15m]));
    }

    [Fact]
    public void ToTransaction_ZeroAmount_DoesNotCreatePointEntry()
    {
        var transaction = CreatePayload(amount: 0m).ToTransaction(Guid.NewGuid());

        Assert.Null(transaction);
    }

    [Fact]
    public void ToTransaction_GuestOrder_DoesNotCreatePointEntry()
    {
        var payload = CreatePayload(amount: 100_000m);
        payload.ObjectId = null;

        Assert.Null(payload.ToTransaction(Guid.NewGuid()));
    }

    [Fact]
    public void ToTransaction_PreservesEventTimeAndEnrichedMetadata()
    {
        var sourceEventId = Guid.NewGuid();
        var payload = CreatePayload(amount: 1001m);

        var transaction = Assert.IsType<Domain.Aggregates.Funds.Transaction>(
            payload.ToTransaction(sourceEventId)
        );
        var metadata = Assert.IsType<Dictionary<string, object?>>(transaction.Metadata);

        Assert.Equal(2m, transaction.Amount);
        Assert.Equal(payload.TransactionAt, transaction.TransactionAt);
        Assert.Equal(payload.ReferenceId, metadata["referenceId"]);
        Assert.Equal(nameof(FundEventType.Order), metadata["fundEventType"]);
        Assert.Equal("earn", metadata["pointAction"]);
        Assert.Equal(sourceEventId, metadata["sourceEventId"]);
        Assert.Equal("ORD-42", metadata["code"]);
    }

    [Fact]
    public void ToTransactionUsePoint_PreservesEventTimeAndMarksSpend()
    {
        var sourceEventId = Guid.NewGuid();
        var payload = CreatePayload(amount: 100_000m);
        payload.Point = 25m;

        var transaction = payload.ToTransactionUsePoint(sourceEventId);
        var metadata = Assert.IsType<Dictionary<string, object?>>(transaction.Metadata);

        Assert.Equal(-25m, transaction.Amount);
        Assert.Equal(payload.TransactionAt, transaction.TransactionAt);
        Assert.Equal("spend", metadata["pointAction"]);
        Assert.Equal(sourceEventId, metadata["sourceEventId"]);
    }

    [Fact]
    public void ToFund_CopiesSourceEventIdAndKeepsManualFundNullable()
    {
        var sourceEventId = Guid.NewGuid();
        var payload = CreatePayload(amount: 10m);

        Assert.Equal(sourceEventId, payload.ToFund(sourceEventId).SourceEventId);
        Assert.Null(payload.ToFund().SourceEventId);
    }

    internal static CreateFundEventPayload CreatePayload(decimal amount)
    {
        return new CreateFundEventPayload
        {
            TypeId = "income",
            BehaviorId = 1,
            ReferenceId = 42,
            Amount = amount,
            PaymentMethod = PaymentMethod.Cash,
            Metadata = new Dictionary<string, object> { ["code"] = "ORD-42" },
            BranchId = 7,
            ObjectId = 99,
            TransactionAt = new DateTimeOffset(2026, 9, 1, 8, 30, 0, TimeSpan.Zero),
            FundEventType = FundEventType.Order,
            Point = 0,
        };
    }
}
