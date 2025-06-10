using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Inventories.Specifications
{
    public class GetInventoryImportWithIncludeByIdSpecification : Specification<InventoryDocument>
    {
        public GetInventoryImportWithIncludeByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id && x.Status != InventoryDocumentStatus.Cancelled)
                .Include(x => x.ProductSupplyings)
                .Include(x => x.EquipmentSupplyings);
        }
    }
}
