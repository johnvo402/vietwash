using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Funds.Specifications
{
    public class ListTransactionSpecification : Specification<Transaction>
    {
        public ListTransactionSpecification()
        {
            Query.OrderByDescending(x => x.TransactionAt).AsNoTracking().AsSplitQuery();
        }
    }
}
