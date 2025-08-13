namespace Application.Feature.Common.Projections.Inventories
{
    public class InventoryReceiptModel
    {
        public string OrgName { get; set; }
        public string OrgAddress { get; set; }
        public string Code { get; set; }
        public DateTimeOffset TransactionAt { get; set; }
        public string BranchName { get; set; }
        public string SupplierName { get; set; }
        public long SupplierId { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Amount { get; set; }
        public string AmountInWords { get; set; }
        public string LogoUrl { get; set; }
        public string StampUrl { get; set; }
        public List<EquipmentSupplyingReceipt> EquipmentSupplyings { get; set; }
        public List<ProductSupplyingReceipt> ProductSupplyings { get; set; }
    }

    public class EquipmentSupplyingReceipt
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Total { get; set; }
    }

    public class ProductSupplyingReceipt
    {
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Price { get; set; }
        public string UnitName { get; set; }
        public string Total { get; set; }
    }
}
