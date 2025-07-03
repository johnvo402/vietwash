using Contracts.Utils;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Funds.Events
{
    public static class UpdateStatusOrderEventMapping
    {
        public static Fund ToFund(this CreateFundEventPayload command)
        {
            string code = Generator.GenerateCode("FU-", 6);
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
                referenceId: command.ReferenceId, // TODO: Replace 0 with the actual referenceId as required
                objectId: command.ObjectId, // Provide a value or null for objectId as required
                metadata: command.Metadata // Provide a value or null for metadata as required
            );
        }
    }
}
