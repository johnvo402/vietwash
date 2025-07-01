using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Funds.Events
{
    public static class UpdateStatusOrderEventMapping
    {
        public static Fund ToFund(this UpdateStatusOrderEventPayload command, string code)
        {
            return new Fund(
                code: code,
                name: null, // Assuming CreateFundCommand has a Name property
                type: Enum.Parse<FundType>(command.TypeId, ignoreCase: true),
                status: FundStatus.Confirmed,
                amount: command.Amount,
                fundBehaviorId: command.BehaviorId,
                transactionDate: null, // Provide a DateTimeOffset? value as required
                paymentMethod: command.PaymentMethod,
                branchId: command.BranchId,
                note: null, // Assuming no note is provided in UpdateStatusOrderEvent
                referenceId: command.OrderId, // TODO: Replace 0 with the actual referenceId as required
                objectId: command.CustomerId, // Provide a value or null for objectId as required
                metadata: new Dictionary<string, object>
                {
                    ["code"] = command.Code,
                    ["publicId"] = command.PublicId.ToString(),
                } // Provide a value or null for metadata as required
            );
        }
    }
}
