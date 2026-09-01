using Application.Features.Transactions;
using Contracts.Utils;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Funds.Events;

public static class UpdateStatusOrderEventMapping
{
    public static Fund ToFund(this CreateFundEventPayload command, Guid? sourceEventId = null)
    {
        var fund = new Fund(
            code: Generator.GenerateCode("FU-", 6),
            name: null,
            type: Enum.Parse<FundType>(command.TypeId, ignoreCase: true),
            status: FundStatus.Confirmed,
            amount: command.Amount,
            fundBehaviorId: command.BehaviorId,
            transactionDate: command.TransactionAt,
            paymentMethod: command.PaymentMethod,
            branchId: command.BranchId,
            note: null,
            referenceId: command.ReferenceId,
            objectId: command.ObjectId,
            metadata: command.Metadata,
            sourceEventId: sourceEventId
        );
        fund.CreatedAt = command.TransactionAt;
        return fund;
    }

    public static Transaction? ToTransaction(
        this CreateFundEventPayload command,
        Guid? sourceEventId = null
    )
    {
        if (command.ObjectId is null)
            return null;

        var earnedPoints = PointCalculator.CalculateEarnedPoints(command.Amount);
        if (earnedPoints == 0)
            return null;

        return new Transaction(
            type: TransactionType.Point,
            amount: earnedPoints,
            transactionAt: command.TransactionAt,
            customerId: command.ObjectId.Value,
            metadata: CreatePointMetadata(command, "earn", sourceEventId)
        );
    }

    public static Transaction ToTransactionUsePoint(
        this CreateFundEventPayload command,
        Guid? sourceEventId = null
    )
    {
        if (command.ObjectId is null)
            throw new ArgumentNullException(nameof(command.ObjectId));

        return new Transaction(
            type: TransactionType.Point,
            amount: -command.Point,
            transactionAt: command.TransactionAt,
            customerId: command.ObjectId.Value,
            metadata: CreatePointMetadata(command, "spend", sourceEventId)
        );
    }

    private static Dictionary<string, object?> CreatePointMetadata(
        CreateFundEventPayload command,
        string pointAction,
        Guid? sourceEventId
    )
    {
        var metadata = (command.Metadata ?? new Dictionary<string, object>()).ToDictionary(
            item => item.Key,
            item => (object?)item.Value
        );

        metadata["referenceId"] = command.ReferenceId;
        metadata["fundEventType"] = command.FundEventType.ToString();
        metadata["pointAction"] = pointAction;
        if (sourceEventId.HasValue)
            metadata["sourceEventId"] = sourceEventId.Value;

        return metadata;
    }
}
