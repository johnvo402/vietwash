using Domain.Aggregates.Inventories.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Inventories.Specifications
{
    public class ListInventoryImportSpecification : Specification<InventoryDocument>
    {
        public ListInventoryImportSpecification()
        {
            Query.Where(x => x.Status != InventoryDocumentStatus.Cancelled)
                 .Include(x => x.ProductSupplyings)
                     .ThenInclude(ps => ps.UnitRelation)
                 .Include(x => x.EquipmentSupplyings)
                     .ThenInclude(es => es.UnitRelation)
                 .AsNoTracking()
                 .AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
