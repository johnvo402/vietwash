using System;
using System.Collections.Generic;
using System.Linq;
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
            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                Query
                    .Where(x =>
                        x.TransactionDate >= DateTime.Parse(from)
                        && x.TransactionDate < DateTime.Parse(to)
                    )
                    .Include(x => x.FundBehavior)
                    .AsNoTracking()
                    .AsSplitQuery();
            }
            else
            {
                Query.Include(x => x.FundBehavior).AsNoTracking().AsSplitQuery();
            }
        }
    }
}
