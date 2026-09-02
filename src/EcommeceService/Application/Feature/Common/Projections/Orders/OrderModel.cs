using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderUpdateStatus
    {
        public OrderStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? CancellationReason { get; set; }
        public List<OrderEquipmentSelectionModel>? OrderEquipments { get; set; } = [];
    }

    public class UpdateOrderModel
    {
        public long TariffId { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? DeliveryTime { get; set; }
        public List<OrderItemSelectionModel> OrderItems { get; set; } = [];
    }
}
