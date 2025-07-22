namespace Application.Feature.Common.Projections.Receipts
{
    public class ReceiptModel
    {
        public CustomerInfoReceipt Customer { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public List<OrderItemReceipt> OrderItems { get; set; }
        public string Total { get; set; }
        public string TotalInWords { get; set; }
    }

    public class CustomerInfoReceipt
    {
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class OrderItemReceipt
    {
        public string ServiceName { get; set; }
        public string UnitRelationName { get; set; }
        public int Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? TotalPriceItem { get; set; }
    }
}
