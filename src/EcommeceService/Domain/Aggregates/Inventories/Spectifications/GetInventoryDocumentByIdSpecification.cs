using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Inventories.Spectifications
{
    public class GetInventoryDocumentByIdSpecification : Specification<InventoryDocument>
    {
        public GetInventoryDocumentByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id)
                .Include(x => x.ProductSupplyings)
                .ThenInclude(x => x.UnitRelation)
                .Include(x => x.ProductSupplyings)
                .ThenInclude(x => x.Supplier)
                .Include(x => x.ProductSupplyings)
                .ThenInclude(x => x.Product)
                .Include(x => x.EquipmentSupplyings)
                .ThenInclude(x => x.Supplier)
                .AsSplitQuery()
                .AsNoTracking();
        }
    }
}
