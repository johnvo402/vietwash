using Domain.Aggregates.Funds;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Users;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Orders
{
    public class Order : AggregateRoot
    {

        public string Code { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public decimal Total { get; set; } = default!;
        public bool DiscountType { get; set; } = default!;
        public decimal DiscountValue { get; set; } = default!;
        public Ulid? CustomerId { get; set; }
        public string Note { get; set; } = default!;
        public OrderStatus Status { get; set; } = default!;
        public DateTimeOffset OrderDate { get; set; } = default!;
        public string PaymentMethodId { get; set; } = default!;
        public PaymentMethod? PaymentMethod { get; set; } = default!;

		public ICollection<OrderPayment> OrderPayments { get; set; } = [];
        public User? Customer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = [];


        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {

                default:
                    return false;
            }
        }
    }
}
