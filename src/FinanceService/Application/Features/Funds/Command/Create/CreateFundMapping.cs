using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Funds.Command.Create
{
    public static class CreateFundMapping
    {
        public static Fund ToFund(this CreateFundCommand command, string code)
        {
            return new Fund(
                code: code,
                name: null, // Assuming CreateFundCommand has a Name property
                type: command.Type,
                status: FundStatus.PendingConfirmation,
                amount: command.Amount,
                fundBehaviorId: command.FundBehaviorId,
                note: command.Note,
                transactionDate: null, // Provide a DateTimeOffset? value as required
                paymentMethod: command.PaymentMethod,
                branchId: command.BranchId,
                referenceId: null, // TODO: Replace 0 with the actual referenceId as required
                objectId: null, // Provide a value or null for objectId as required
                metadata: null // Provide a value or null for metadata as required
            );
        }
    }
}
