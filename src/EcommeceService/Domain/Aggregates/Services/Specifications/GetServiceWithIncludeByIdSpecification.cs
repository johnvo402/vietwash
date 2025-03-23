using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Services.Specifications
{
    public class GetServiceWithIncludeByIdSpecification : Specification<Service>
    {
        public GetServiceWithIncludeByIdSpecification(Ulid id)
        {
            Query.Where(x => x.Id == id).Include(x=>x.UnitRelations).AsNoTracking();
        }
    }
}
