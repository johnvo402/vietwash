namespace Application.Feature.Common.Projections.Inventories
{
    public class InventoryReceiptModel
    {
        public string OrgName { get; set; } = default!;
        public string OrgAddress { get; set; } = default!;
        public string Code { get; set; } = default!;
        public DateTimeOffset TransactionAt { get; set; }
        public string BranchName { get; set; } = default!;
        public string SupplierName { get; set; } = default!;
        public long SupplierId { get; set; }
        public string CreatedBy { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public string Amount { get; set; } = default!;
        public string AmountInWords { get; set; } = default!;
        public string LogoUrl { get; set; } = default!;
        public string StampUrl { get; set; } = default!;
        public List<EquipmentSupplyingReceipt> EquipmentSupplyings { get; set; } = default!;
        public List<ProductSupplyingReceipt> ProductSupplyings { get; set; } = default!;
    }

    public class EquipmentSupplyingReceipt
    {
        public string Name { get; set; } = default!;
        public string Price { get; set; } = default!;
        public int Quantity { get; set; }
        public string Total { get; set; } = default!;
    }

    public class ProductSupplyingReceipt
    {
        public string ProductName { get; set; } = default!;
        public decimal Quantity { get; set; }
        public string Price { get; set; } = default!;
        public string UnitName { get; set; } = default!;
        public string Total { get; set; } = default!;
    }
}
