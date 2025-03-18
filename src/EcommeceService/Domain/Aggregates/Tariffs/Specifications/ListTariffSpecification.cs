using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common.Specs;

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