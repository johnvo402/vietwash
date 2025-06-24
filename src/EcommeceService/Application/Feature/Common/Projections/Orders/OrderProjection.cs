using Contracts.Application.Common;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderProjection : BaseResponse
    {
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public bool DiscountFixed { get; set; }
        public decimal DiscountValue { get; set; }
        public long? CustomerId { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset DeliveryTime { get; set; }
        public OrderStatus Status { get; set; }
        public long BranchId { get; set; }
    }
}
