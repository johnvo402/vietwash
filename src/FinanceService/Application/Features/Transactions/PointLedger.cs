using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Transactions;

public static class PointLedger
{
    public static async Task<decimal> GetBalanceAsync(
        IQueryable<Transaction> transactions,
        long customerId,
        CancellationToken cancellationToken = default
    )
    {
        return await transactions
                .Where(x => x.CustomerId == customerId && x.Type == TransactionType.Point)
                .Select(x => (decimal?)x.Amount)
                .SumAsync(cancellationToken)
            ?? 0m;
    }

    public static decimal CalculateBalance(IEnumerable<decimal> amounts) => amounts.Sum();
}
