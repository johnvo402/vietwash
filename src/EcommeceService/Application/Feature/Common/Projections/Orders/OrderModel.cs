using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderModel
    {
        public long? CustomerId { get; set; }
        public long BranchId { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? DeliveryTime { get; set; }
        public bool DiscountFixed { get; set; } // true = percentage, false = fixed amount
        public decimal DiscountValue { get; set; }
        public List<OrderItemModel> OrderItems { get; set; } = [];
    }
}
