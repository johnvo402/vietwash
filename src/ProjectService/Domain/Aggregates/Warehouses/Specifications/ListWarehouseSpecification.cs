using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Warehouses.Specifications
{
    public class ListWarehouseSpecification : Specification<Warehouse>
    {
        public ListWarehouseSpecification()
        {
            Query.AsNoTracking().AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
