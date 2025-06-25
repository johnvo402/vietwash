using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Tariffs.Specifications
{
    public class ListTariffSpecification : Specification<Tariff>
    {
        public ListTariffSpecification()
        {
            Query.AsNoTracking().AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}