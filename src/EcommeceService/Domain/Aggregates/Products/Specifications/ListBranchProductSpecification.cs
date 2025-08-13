using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Aggregates.Inventories.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Products.Specifications
{
    public class ListBranchProductSpecification : Specification<BranchProduct>
    {
        public ListBranchProductSpecification()
        {
            Query
                .Where(x => !x.Disable)
                .Include(x => x.Category)
                .Include(x => x.UnitRelations)
                .Include(s => s.ProductSupplyings)
                .ThenInclude(ps => ps.UnitRelation)
                .Include(s => s.ProductSupplyings)
                .ThenInclude(x => x.InventoryDocument)
                .AsSplitQuery()
                .AsNoTracking();
        }
    }
}
