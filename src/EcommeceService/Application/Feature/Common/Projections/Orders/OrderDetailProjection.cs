using Application.Feature.Common.Projections.Services;
using Application.Feature.Services.Queries.Detail;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderDetailProjection : OrderProjection
    {
        public List<OrderItemProjection> OrderItems { get; set; } = [];
        public List<OrderPaymentProjection> OrderPayments { get; set; } = [];
        public UserDTO? Customer { get; set; }
    }

    public class OrderItemProjection
    {
        public string Id { get; set; }
        public long OrderId { get; set; }
        public long ServiceId { get; set; }
        public long UnitRelationId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? UnitRelationName { get; set; }
        public decimal ProcessingTime { get; set; }
        public string? ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
        public ServiceModel Service { get; set; }
    }

    public class OrderPaymentProjection
    {
        public long OrderId { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; }

        public decimal Amount { get; set; } = default!;

        public DateTimeOffset PaymentDate { get; set; }
    }
}
