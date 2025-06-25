using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Tariffs.Specifications
{
    public class GetTariffByIdWithoutIncludeSpecification : Specification<Tariff>
    {
        public GetTariffByIdWithoutIncludeSpecification(long id)
        {
            Query.Where(x => x.Id == id).AsNoTracking();
        }
    }
}