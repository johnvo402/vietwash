using System.Linq.Expressions;
using Application.Features.Common.Mapping;
using Domain.Aggregates.Funds;

namespace Application.Features.Transactions.Queries.List
{
    public static class ListTransactionMapping
    {
        public static Expression<Func<Transaction, ListTransactionResponse>> Selector()
        {
            return transaction => new ListTransactionResponse
            {
                Id = transaction.Id,
                PublicId = transaction.PublicId,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Metadata = transaction.Metadata,
                TransactionAt = transaction.TransactionAt,

                Customer =
                    transaction.Customer != null ? transaction.Customer.ToUserDTOResponse() : null,
            };
        }
    }
}
