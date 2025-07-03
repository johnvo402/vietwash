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
                .Include(x => x.EquipmentSupplyings)
                .AsSplitQuery()
                .AsNoTracking();
        }
    }
}
