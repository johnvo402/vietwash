using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Specification;
using Specification.Builders;

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
