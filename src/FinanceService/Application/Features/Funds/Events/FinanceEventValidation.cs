using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Funds.Events;

public static class FinanceEventValidation
{
    public static string? Validate(UpdateStatusOrderEvent request)
    {
        if (request.PayloadId == Guid.Empty)
            return "PayloadId is required.";

        if (request.Payload is null)
            return "Payload is required.";

        var payload = request.Payload;
        if (
            !Enum.TryParse<FundType>(payload.TypeId, ignoreCase: true, out var fundType)
            || !Enum.IsDefined(fundType)
        )
            return "TypeId is invalid.";

        if (payload.Amount < 0)
            return "Amount cannot be negative.";

        if (payload.Point < 0)
            return "Point cannot be negative.";

        if (payload.BehaviorId <= 0)
            return "BehaviorId must be positive.";

        if (payload.ReferenceId <= 0)
            return "ReferenceId must be positive.";

        if (payload.BranchId <= 0)
            return "BranchId must be positive.";

        if (payload.ObjectId <= 0)
            return "ObjectId must be positive when provided.";

        if (payload.TransactionAt == default)
            return "TransactionAt is required.";

        if (!Enum.IsDefined(payload.PaymentMethod))
            return "PaymentMethod is invalid.";

        if (!Enum.IsDefined(payload.FundEventType))
            return "FundEventType is invalid.";

        if (
            payload.FundEventType == Features.Common.Enums.FundEventType.Order
            && payload.Point > 0
            && payload.ObjectId is null
        )
            return "ObjectId is required when points are spent.";

        return null;
    }
}
