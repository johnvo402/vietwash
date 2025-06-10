using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Suppliers.Enum;
using System.ComponentModel.DataAnnotations;


namespace Application.Feature.Common.Projections.InventoryImports
{
    public class InventoryImportUpdateModel
    {
        public InventoryDocumentStatus? Status { get; set; }
        public InventoryImportModel InventoryImportModel { get; set; } = default!;
    }
}
