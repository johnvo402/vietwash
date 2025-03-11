using Domain.Aggregates.Funds;
using Domain.Aggregates.Users;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Orders
{
    public class Order : AggregateRoot
    {

        public string Code { get; set; } = default!;
        public long Amount { get; set; } = default!;
        public long Total { get; set; } = default!;
        public bool DiscountType { get; set; } = default!;
        public double DiscountValue { get; set; } = default!;
        public Ulid CustomerId { get; set; } = default!;
        public string Note { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string PaymentMethodId { get; set; } = default!;

        public virtual OrderPayment OrderPayment { get; set; }=default!;
        public virtual User User { get; set; }=default!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];



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
