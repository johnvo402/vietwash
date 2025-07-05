using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Tariffs.Specifications
{
    public class ListTariffSpecification : Specification<Tariff>
    {
        public ListTariffSpecification()
        {
            Query
                .Where(x => !x.Disable)
                .AsNoTracking()
                .AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}