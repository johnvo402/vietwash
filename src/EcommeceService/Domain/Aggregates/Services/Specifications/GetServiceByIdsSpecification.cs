using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Services.Specifications
{
    public class GetServiceByIdsSpecification : Specification<Service>
    {
        public GetServiceByIdsSpecification(List<Ulid> serviceIds)
        {
            Query.Where(s => serviceIds.Contains(s.Id));
        }
    }
}
