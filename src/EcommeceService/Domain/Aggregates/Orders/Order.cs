using Ardalis.GuardClauses;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Events;
using Domain.Events;
using Domain.Events.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Orders
{
    public class Order : AggregateRoot
    {
        public long? CustomerId { get; set; }
        public long BranchId { get; set; } = default!;
        public long StaffId { get; set; } = default!;
        public long? VoucherId { get; set; }
        public long? TariffId { get; set; }
        public string? VoucherCode { get; set; }
        public string Code { get; set; } = default!;
        public decimal Amount { get; set; } = default!;

        public int Vat { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; } = default!;

        /// <summary>True for a fixed monetary discount; false for a percentage.</summary>
        public bool DiscountFixed { get; set; } = default!;
        public PaymentMethod? PaymentMethod { get; set; }
        public decimal DiscountValue { get; set; } = default!;
        public decimal Point { get; set; } = 0;
        public string Note { get; set; } = default!;
        public OrderStatus Status { get; set; } = default!;
        public DateTimeOffset? OrderDate { get; set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public long? CancelledBy { get; private set; }
        public string? CancellationReason { get; private set; }
        public DateTimeOffset DeliveryTime { get; set; } = default!;
        public User? Staff { get; set; }
        public User? Customer { get; set; }
        public Tariff? Tariff { get; set; }
        public virtual VoucherUsage? VoucherUsage { get; set; }

        public string? CodeConfirm { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = [];

        public ICollection<OrderEquipment> OrderEquipments { get; set; } = [];

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {
                case CreateFundEvent:
                    return true;
                case VoucherUsageEvent:
                    return true;
                case UpdateStatusOrderEvent:
                    return true;
                case EInvoiceEvent:
                    return true;
                default:
                    return false;
            }
        }

        private Order() { }

        public Order(
            long branchId,
            long staffId,
            string code,
            decimal amount,
            decimal total,
            OrderStatus status,
            int vat = 0,
            decimal vatAmount = 0,
            long? voucherId = null,
            string? voucherCode = null,
            long? customerId = null,
            bool discountFixed = false,
            decimal discountValue = 0,
            decimal point = 0,
            string? note = null,
            long? tariffId = null,
            DateTimeOffset? deliveryTime = null
        )
        {
            Guard.Against.Null(code, nameof(code));
            Guard.Against.Null(status, nameof(status));
            Vat = vat;
            VatAmount = vatAmount;
            BranchId = branchId;
            StaffId = staffId;
            VoucherId = voucherId;
            VoucherCode = voucherCode;
            Code = code;
            Amount = amount;
            Total = total;
            Status = status;

            CustomerId = customerId;
            DiscountFixed = discountFixed;
            DiscountValue = discountValue;
            Note = note ?? string.Empty;
            DeliveryTime = deliveryTime ?? DateTimeOffset.UtcNow.AddDays(1);
            Point = point;
            TariffId = tariffId;
        }

        public void Update(
            decimal? amount = null,
            decimal? total = null,
            decimal? point = null,
            decimal? vatAmount = null,
            string? note = null,
            long? tariffId = null,
            DateTimeOffset? deliveryTime = null
        )
        {
            if (note != null)
                Note = note;

            if (deliveryTime.HasValue)
                DeliveryTime = deliveryTime.Value;

            if (amount.HasValue)
                Amount = amount.Value;
            if (vatAmount.HasValue)
                VatAmount = vatAmount.Value;

            if (total.HasValue)
                Total = total.Value;

            if (point.HasValue)
                Point = point.Value;

            if (tariffId.HasValue)
                TariffId = tariffId;
        }

        public OrderTransitionResult EvaluateTransition(
            OrderStatus target,
            Enums.PaymentMethod? paymentMethod,
            int equipmentCount,
            OrderCancellation? cancellation = null
        )
        {
            if (Status == target)
                return OrderTransitionResult.Idempotent;

            if (!OrderLifecycle.CanTransition(Status, target))
                return OrderTransitionResult.InvalidTransition;

            if (target == OrderStatus.Completed)
            {
                if (!paymentMethod.HasValue || !Enum.IsDefined(paymentMethod.Value))
                    return OrderTransitionResult.PaymentMethodRequired;
            }
            else if (paymentMethod.HasValue)
                return OrderTransitionResult.PaymentMethodNotAllowed;

            if (target == OrderStatus.InProgress && equipmentCount == 0)
                return OrderTransitionResult.EquipmentRequired;

            if (target != OrderStatus.InProgress && equipmentCount != 0)
                return OrderTransitionResult.EquipmentNotAllowed;

            if (target == OrderStatus.Cancelled && cancellation is null)
                return OrderTransitionResult.CancellationRequired;

            if (target != OrderStatus.Cancelled && cancellation is not null)
                return OrderTransitionResult.CancellationNotAllowed;

            return OrderTransitionResult.Applied;
        }

        public OrderTransitionResult TransitionTo(
            OrderStatus target,
            Enums.PaymentMethod? paymentMethod = null,
            IReadOnlyCollection<OrderEquipment>? orderEquipments = null,
            DateTimeOffset? transitionedAt = null,
            OrderCancellation? cancellation = null
        )
        {
            int equipmentCount = orderEquipments?.Count ?? OrderEquipments.Count;
            OrderTransitionResult validation = EvaluateTransition(
                target,
                paymentMethod,
                equipmentCount,
                cancellation
            );
            if (validation != OrderTransitionResult.Applied)
                return validation;

            if (target == OrderStatus.InProgress && orderEquipments is not null)
                foreach (OrderEquipment equipment in orderEquipments!)
                    OrderEquipments.Add(equipment);

            if (target == OrderStatus.Completed)
            {
                PaymentMethod = paymentMethod!.Value;
                OrderDate = transitionedAt ?? DateTimeOffset.UtcNow;
            }

            if (target == OrderStatus.Cancelled)
            {
                CancelledAt = cancellation!.CancelledAt;
                CancelledBy = cancellation.CancelledBy;
                CancellationReason = cancellation.Reason;
            }

            Status = target;
            Emit(new UpdateStatusOrderEvent { Order = this });

            if (target == OrderStatus.Completed)
            {
                Emit(new EInvoiceEvent { Order = this });
                Emit(
                    new CreateFundEvent
                    {
                        TypeId = "income",
                        ReferenceId = Id,
                        Amount = Total,
                        PaymentMethod = PaymentMethod!.Value,
                        TransactionAt = OrderDate!.Value,
                        BranchId = BranchId,
                        ObjectId = CustomerId,
                        BehaviorId = 1,
                        Metadata = new Dictionary<string, object>
                        {
                            ["code"] = Code,
                            ["publicId"] = PublicId.ToString(),
                            ["type"] = FundEventType.Order,
                        },
                        Point = Point,
                        FundEventType = FundEventType.Order,
                    }
                );

                if (VoucherId.HasValue && CustomerId.HasValue)
                    Emit(
                        new VoucherUsageEvent
                        {
                            VoucherId = VoucherId.Value,
                            CustomerId = CustomerId.Value,
                            BranchId = BranchId,
                            OrderId = Id,
                            DiscountApply = DiscountValue,
                        }
                    );
            }

            return OrderTransitionResult.Applied;
        }
    }
}
