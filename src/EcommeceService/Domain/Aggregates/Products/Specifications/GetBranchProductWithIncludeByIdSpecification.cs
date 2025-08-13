using Domain.Aggregates.Inventories.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Products.Specifications
{
    public class GetBranchProductWithIncludeByIdSpecification : Specification<BranchProduct>
    {
        public GetBranchProductWithIncludeByIdSpecification(long id)
        {
            Query
                .Where(s => s.Id == id && !s.Disable)
                .Include(s => s.UnitRelations)
                .ThenInclude(ur => ur.Unit)
                .Include(s => s.ProductSupplyings)
                .ThenInclude(ps => ps.UnitRelation)
                .Include(s => s.ProductSupplyings)
                .ThenInclude(x => x.InventoryDocument)
                .Include(s => s.Category);
        }
    }
}
