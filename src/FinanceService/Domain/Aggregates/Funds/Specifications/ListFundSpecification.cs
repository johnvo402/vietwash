using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Funds.Specifications
{
    public class ListFundSpecification : Specification<Fund>
    {
        public ListFundSpecification(string? from, string? to)
        {
            Query.Include(x => x.FundBehavior).AsNoTracking().AsSplitQuery();

            var filter = BuildDateFilter(from, to);

            if (filter is not null)
            {
                Query.Where(filter);
            }
        }

        private static Expression<Func<Fund, bool>>? BuildDateFilter(string? from, string? to)
        {
            if (DateTime.TryParse(from, out var fromDate) && DateTime.TryParse(to, out var toDate))
            {
                return x =>
                    !x.TransactionDate.HasValue
                    || (x.TransactionDate >= fromDate && x.TransactionDate < toDate);
            }

            return null;
        }
    }
}
