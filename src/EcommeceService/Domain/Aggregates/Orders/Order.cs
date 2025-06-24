using Ardalis.GuardClauses;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Users;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Orders
{
    public class Order : AggregateRoot
    {
        public long? CustomerId { get; set; }
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

        public Order(
            long branchId,
            long staffId,
            string code,
            decimal amount,
            decimal total,
            OrderStatus status,
            DateTimeOffset orderDate,
            long? customerId = null,
            bool discountFixed = false,
            decimal discountValue = 0,
            string? note = null,
            DateTimeOffset? deliveryTime = null
        )
        {
            Guard.Against.Null(code, nameof(code));
            Guard.Against.Null(status, nameof(status));

            BranchId = branchId;
            StaffId = staffId;
            Code = code;
            Amount = amount;
            Total = total;
            Status = status;
            OrderDate = orderDate;

            CustomerId = customerId;
            DiscountFixed = discountFixed;
            DiscountValue = discountValue;
            Note = note ?? string.Empty;
            DeliveryTime = deliveryTime ?? orderDate.AddDays(1);
        }

        public void Update(
            long? customerId = null,
            long? branchId = null,
            long? staffId = null,
            string? code = null,
            decimal? amount = null,
            decimal? total = null,
            bool? discountFixed = null,
            decimal? discountValue = null,
            string? note = null,
            OrderStatus? status = null,
            DateTimeOffset? orderDate = null,
            DateTimeOffset? deliveryTime = null
        )
        {
            if (code is not null)
                Guard.Against.NullOrWhiteSpace(code, nameof(code));

            if (status.HasValue)
                Guard.Against.Null(status, nameof(status));

            if (customerId.HasValue)
                CustomerId = customerId.Value;
            if (branchId.HasValue)
                BranchId = branchId.Value;
            if (staffId.HasValue)
                StaffId = staffId.Value;
            if (code is not null)
                Code = code;
            if (amount.HasValue)
                Amount = amount.Value;
            if (total.HasValue)
                Total = total.Value;
            if (discountFixed.HasValue)
                DiscountFixed = discountFixed.Value;
            if (discountValue.HasValue)
                DiscountValue = discountValue.Value;
            if (note is not null)
                Note = note;
            if (status.HasValue)
                Status = status.Value;
            if (orderDate.HasValue)
                OrderDate = orderDate.Value;
            if (deliveryTime.HasValue)
                DeliveryTime = deliveryTime.Value;
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
                            PaymentMethod = OrderPayments.FirstOrDefault()!.PaymentMethod,
                            ReferenceId = Id,
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
                            PaymentMethod = OrderPayments.FirstOrDefault()!.PaymentMethod,
                            ReferenceId = Id,
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
