using Domain.Aggregates.Orders.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderPayment : DefaultEntity
    {
        public long OrderId { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; }

        public decimal Amount { get; set; } = default!;

        public DateTimeOffset PaymentDate { get; set; }

        public Order? Order { get; set; }
    }
}
