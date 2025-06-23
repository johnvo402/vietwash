using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Orders
{
    public class Order : AggregateRoot
    {
        public long CustomerId { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public long StaffId { get; set; } = default!;
        public string Code { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public decimal Total { get; set; } = default!;
        public bool DiscountFixed { get; set; } = default!;
        public decimal DiscountValue { get; set; } = default!;
        public string Note { get; set; } = default!;
        public OrderStatus Status { get; set; } = default!;
        public DateTimeOffset OrderDate { get; set; } = default!;
        public DateTimeOffset DeliveryTime { get; set; } = default!;
        public User? Staff { get; set; }
        public User? Customer { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = [];
        public ICollection<OrderPayment> OrderPayments { get; set; } = [];

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {
                case UpdateStatusOrderEvent:
                    return true;
                default:
                    return false;
            }
        }

        public void UpdateStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Completed:
                    Status = OrderStatus.Completed;
                    Emit(
                        new UpdateStatusOrderEvent()
                        {
                            TypeId = "income",
                            BehaviorId = "order",
                            Amount = Total,
                            PaymentMethod = this.OrderPayments.FirstOrDefault()!.PaymentMethod,
                            ReferenceId = this.Id,
                        }
                    );
                    break;
                case OrderStatus.Cancelled:
                    Status = OrderStatus.Cancelled;
                    Emit(
                        new UpdateStatusOrderEvent()
                        {
                            TypeId = "expense",
                            BehaviorId = "order_cancelled",
                            Amount = Total,
                            PaymentMethod = this.OrderPayments.FirstOrDefault()!.PaymentMethod,
                            ReferenceId = this.Id,
                        }
                    );
                    break;
                default:
                    Status = status;
                    break;
            }
        }
    }
}
