using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Aggregates.Tariffs;
using JohnChum.SharedKernel.Domain.Common.Specs;

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