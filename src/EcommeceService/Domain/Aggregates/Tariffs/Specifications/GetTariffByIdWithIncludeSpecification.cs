using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Tariffs.Specifications
{
    public class GetTariffByIdWithIncludeSpecification : Specification<Tariff>
    {
        public GetTariffByIdWithIncludeSpecification(long id)
        {
            Query
                .Where(x => x.Id == id && !x.Disable)
                .Include(x => x.ServiceTariffs);
        }
    }
}