using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Warehouses.Specifications
{
    public class GetWarehouseByIdWithoutIncludeSpecification : Specification<Warehouse>
    {
        public GetWarehouseByIdWithoutIncludeSpecification(long id)
        {
            Query.Where(x => x.Id == id).AsNoTracking();
        }
    }
}
