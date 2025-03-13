using Domain.Aggregates.Funds;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderPayment : DefaultEntity
    {
        public Ulid OrderId { get; set; } = default!;
        public string PaymentMethodId { get; set; } = default!;

        public decimal Amount { get; set; } = default!;

        public DateTimeOffset PaymentDate { get; set; }

        public Order? Order { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}
