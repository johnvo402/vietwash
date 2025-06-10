using Domain.Aggregates.Inventories.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Inventories.Specifications
{
    public class GetInventoryDocumentByIdSpec : Specification<InventoryDocument>
    {
        public GetInventoryDocumentByIdSpec(long id)
        {
            Query.Where(x => x.Id == id && x.Status != InventoryDocumentStatus.Cancelled)
                 .Include(x => x.ProductSupplyings)
                 .Include(x => x.EquipmentSupplyings);
        }
    }
}
