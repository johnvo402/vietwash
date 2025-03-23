using Domain.Aggregates.Funds;
using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderPayment : DefaultEntity
    {
        public Ulid OrderId { get; set; } = default!;
		public PaymentMethod PaymentMethod { get; set; }

		public decimal Amount { get; set; } = default!;

        public DateTimeOffset PaymentDate { get; set; }

        public Order? Order { get; set; }
    }
}
