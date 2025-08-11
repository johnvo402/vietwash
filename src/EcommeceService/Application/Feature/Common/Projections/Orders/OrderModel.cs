using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderModel
    {
        public long? CustomerId { get; set; }
        public long BranchId { get; set; }
        public long TariffId { get; set; }
        public decimal Point { get; set; }
        public string? VoucherCode { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? DeliveryTime { get; set; }
        public bool DiscountFixed { get; set; } // true = percentage, false = fixed amount
        public decimal DiscountValue { get; set; }
        public List<OrderItemModel> OrderItems { get; set; } = [];
    }

    public class OrderUpdateStatus
    {
        public OrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public List<OrderEquipmentModel>? OrderEquipments { get; set; } = [];
    }

    public class UpdateOrderModel
    {
        public long TariffId { get; set; }
        public decimal Point { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? DeliveryTime { get; set; }
        public List<OrderItemModel> OrderItems { get; set; } = [];
    }
}
