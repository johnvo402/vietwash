using Application.Feature.Common.Mapping.Inventories;
using Contracts.Application.Common;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;

namespace Application.Feature.Common.Projections.Inventories
{
    public class InventoryDocumentProjection : BaseResponse
    {
        public decimal Amount { get; set; }
        public long? BranchId { get; set; }
        public DateTimeOffset? TransactionAt { get; set; }
        public string Code { get; set; } = null!;
        public InventoryStatus Status { get; set; }
        public string? ArrivedAt { get; set; }
        public InventoryType Type { get; set; }

        public virtual void MappingFrom(InventoryDocument document)
        {
            Id = document.Id;
            PublicId = document.PublicId;
            CreatedAt = document.CreatedAt;
            CreatedBy = document.CreatedBy;
            UpdatedAt = document.UpdatedAt;
            UpdatedBy = document.UpdatedBy;

            Amount = document.Amount;
            BranchId = document.BranchId;
            TransactionAt = document.TransactionAt;
            Code = document.Code;
            Status = document.Status;
            Type = document.Type;
        }
    }

    public class InventoryDocumentDetailProjection : InventoryDocumentProjection
    {
        public string? Note { get; set; }
        public ICollection<EquipmentSupplyingProjection> EquipmentSupplyings { get; set; } = [];
        public ICollection<ProductSupplyingProjection> ProductSupplyings { get; set; } = [];

        public override void MappingFrom(InventoryDocument inventoryDocument)
        {
            base.MappingFrom(inventoryDocument);
            Note = inventoryDocument.Note;
            EquipmentSupplyings =
                inventoryDocument.EquipmentSupplyings.ToListEquipmentSupplyingProjection();

            ProductSupplyings =
                inventoryDocument.ProductSupplyings.ToListProductSupplyingProjection();
        }
    }
}
