using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Inventories.Spectifications
{
    public class ListInventoryDocumentSpecification : Specification<InventoryDocument>
    {
        public ListInventoryDocumentSpecification(List<long> branchs)
        {
            Expression<Func<InventoryDocument, bool>> criteria = x =>
                x.BranchId.HasValue && branchs.Contains(x.BranchId.Value);

            Query.Where(criteria).AsNoTracking().AsSplitQuery();
        }
    }
}
