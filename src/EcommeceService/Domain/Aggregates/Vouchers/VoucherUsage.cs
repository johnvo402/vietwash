using Ardalis.GuardClauses;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Vouchers.Events;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Vouchers
{
    public class VoucherUsage : AggregateRoot
    {
        public long VoucherId { get; set; }
        public long CustomerId { get; set; }
        public long OrderId { get; set; }
        public decimal DiscountApply { get; set; }
        public virtual Order Order { get; set; } = default!;
        protected VoucherUsage() { }

        public VoucherUsage(long voucherId, long customerId, long orderId, decimal discountApply)
        {
            VoucherId = Guard.Against.NegativeOrZero(voucherId, nameof(voucherId));
            CustomerId = Guard.Against.NegativeOrZero(customerId, nameof(customerId));
            OrderId = Guard.Against.NegativeOrZero(orderId, nameof(orderId));
            DiscountApply = Guard.Against.NegativeOrZero(discountApply, nameof(discountApply));
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
