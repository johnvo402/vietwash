using Ardalis.GuardClauses;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Domain.Events;
using Domain.Events.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Orders
{
    public class Order : AggregateRoot
    {
        private readonly List<OrderItem> _orderItems = [];
        private readonly List<OrderEquipment> _orderEquipments = [];

        public long? CustomerId { get; private set; }
        public long BranchId { get; private set; }
        public long StaffId { get; private set; }
        public long? VoucherId { get; private set; }
        public long? TariffId { get; private set; }
        public string? VoucherCode { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }

        public int Vat { get; private set; }
        public decimal VatAmount { get; private set; }
        public decimal Total { get; private set; }

        /// <summary>True for a fixed monetary discount; false for a percentage.</summary>
        public bool DiscountFixed { get; private set; }
        public PaymentMethod? PaymentMethod { get; private set; }
        public decimal DiscountValue { get; private set; }
        public decimal Point { get; private set; }
        public string Note { get; private set; } = string.Empty;
        public OrderStatus Status { get; private set; }
        public DateTimeOffset? OrderDate { get; private set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public long? CancelledBy { get; private set; }
        public string? CancellationReason { get; private set; }
        public DateTimeOffset DeliveryTime { get; private set; }
        public User? Staff { get; private set; }
        public User? Customer { get; private set; }
        public Tariff? Tariff { get; private set; }
        public virtual VoucherUsage? VoucherUsage { get; private set; }

        public string? CodeConfirm { get; private set; }

        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public IReadOnlyCollection<OrderEquipment> OrderEquipments =>
            _orderEquipments.AsReadOnly();

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {
                case CreateFundEvent:
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
            DateTimeOffset? deliveryTime = null,
            PaymentMethod? paymentMethod = null,
            DateTimeOffset? orderDate = null,
            string? codeConfirm = null,
            IEnumerable<OrderItem>? orderItems = null,
            IEnumerable<OrderEquipment>? orderEquipments = null
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
            PaymentMethod = paymentMethod;
            OrderDate = orderDate;
            CodeConfirm = codeConfirm;
            if (orderItems is not null)
                _orderItems.AddRange(orderItems);
            if (orderEquipments is not null)
                _orderEquipments.AddRange(orderEquipments);
        }

        public void UpdateDetails(
            decimal? amount = null,
            decimal? total = null,
            decimal? point = null,
            decimal? vatAmount = null,
            string? note = null,
            long? tariffId = null,
            DateTimeOffset? deliveryTime = null
        )
        {
            if (!OrderLifecycle.CanEditDetails(Status))
                throw new InvalidOperationException("Only pending orders can be updated.");

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

        public void ReplaceItems(IEnumerable<OrderItem> items)
        {
            if (!OrderLifecycle.CanEditDetails(Status))
                throw new InvalidOperationException("Only pending orders can be updated.");

            ArgumentNullException.ThrowIfNull(items);
            OrderItem[] replacement = items.ToArray();
            _orderItems.Clear();
            _orderItems.AddRange(replacement);
        }

        public void SetConfirmationCode(string? confirmationCode)
        {
            if (!OrderLifecycle.CanEditDetails(Status))
                throw new InvalidOperationException(
                    "A confirmation code can only be assigned to a pending order."
                );

            CodeConfirm = confirmationCode;
        }

        public void AdvanceVersion() => Version = checked(Version + 1);

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
                    _orderEquipments.Add(equipment);

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
            }

            return OrderTransitionResult.Applied;
        }
    }
}
