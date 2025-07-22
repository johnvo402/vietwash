using Application.Features.Common.Projections.Users;

namespace Application.Feature.Common.Projections.Receipts
{
    public class ReceiptModel
    {
        public OrganizationInfo? OrgInfo { get; set; }
        public UserDTO Customer { get; set; }
        public UserDTO Staff { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public List<OrderItemReceipt> OrderItems { get; set; }
        public string Total { get; set; }
        public string TotalInWords { get; set; }
    }

    public class OrganizationInfo
    {
        public string? Logo { get; set; }
        public string? Stamp { get; set; }
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
