using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Inventories.Spectifications
{
    public class GetInventoryDocumentByIdWithoutIncludeSpecification
        : Specification<InventoryDocument>
    {
        public GetInventoryDocumentByIdWithoutIncludeSpecification(long id)
        {
            Query.Where(x => x.Id == id).AsNoTracking();
        }
    }
}
