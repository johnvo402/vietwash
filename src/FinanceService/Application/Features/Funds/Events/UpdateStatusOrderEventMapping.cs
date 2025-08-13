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
            var fund = new Fund(
                code: code,
                name: null, // Assuming CreateFundCommand has a Name property
                type: Enum.Parse<FundType>(command.TypeId, ignoreCase: true),
                status: FundStatus.Confirmed,
                amount: command.Amount,
                fundBehaviorId: command.BehaviorId,
                transactionDate: command.TransactionAt, // Provide a DateTimeOffset? value as required
                paymentMethod: command.PaymentMethod,
                branchId: command.BranchId,
                note: null, // Assuming no note is provided in UpdateStatusOrderEvent
                referenceId: command.ReferenceId, // TODO: Replace 0 with the actual referenceId as required
                objectId: command.ObjectId, // Provide a value or null for objectId as required
                metadata: command.Metadata // Provide a value or null for metadata as required
            );
            fund.CreatedAt = command.TransactionAt;
            return fund;
        }

        public static Transaction ToTransaction(this CreateFundEventPayload command)
        {
            if (command.ObjectId is null)
                throw new ArgumentNullException(nameof(command.ObjectId));

            var metadata = (command.Metadata ?? new Dictionary<string, object>()).ToDictionary(
                kvp => kvp.Key,
                kvp => (object?)kvp.Value
            );

            metadata["id"] = command.ReferenceId;
            return new Transaction(
                type: TransactionType.Point,
                amount: Math.Ceiling(command.Amount / 1000),
                transactionAt: DateTimeOffset.UtcNow, // Provide a DateTimeOffset? value as required
                customerId: (long)command.ObjectId!, // Provide a value or null for objectId as required
                metadata: command.Metadata // Provide a value or null for metadata as required
            );
        }

        public static Transaction ToTransactionUsePoint(this CreateFundEventPayload command)
        {
            if (command.ObjectId is null)
                throw new ArgumentNullException(nameof(command.ObjectId));

            var metadata = (command.Metadata ?? new Dictionary<string, object>()).ToDictionary(
                kvp => kvp.Key,
                kvp => (object?)kvp.Value
            );

            metadata["id"] = command.ReferenceId;
            return new Transaction(
                type: TransactionType.Point,
                amount: -command.Point,
                transactionAt: DateTimeOffset.UtcNow,
                customerId: (long)command.ObjectId!,
                metadata: command.Metadata
            );
        }
    }
}
