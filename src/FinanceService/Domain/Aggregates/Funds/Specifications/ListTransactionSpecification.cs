using System.Linq.Expressions;
using Shared.Kernel.Extentions;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Funds.Specifications
{
    public class ListTransactionSpecification : Specification<Transaction>
    {
        public ListTransactionSpecification(long? customerId = null)
        {
            Expression<Func<Transaction, bool>> criteria = x => true;

            if (customerId.HasValue)
            {
                criteria = criteria.And(x => x.CustomerId == customerId.Value);
            }
            Query
                .Where(criteria)
                .OrderByDescending(x => x.TransactionAt)
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
