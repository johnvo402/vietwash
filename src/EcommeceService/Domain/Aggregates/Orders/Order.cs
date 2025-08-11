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
        public bool DiscountFixed { get; set; } = default!;
        public PaymentMethod? PaymentMethod { get; set; }
        public decimal DiscountValue { get; set; } = default!;
        public decimal Point { get; set; } = 0;
        public string Note { get; set; } = default!;
        public OrderStatus Status { get; set; } = default!;
        public DateTimeOffset? OrderDate { get; set; }
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
                case UseEquipmentOrder:
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

        public void EmitVoucherUsageEvent(decimal discountApply, long voucherId)
        {
            if (CustomerId.HasValue)
            {
                Emit(
                    new VoucherUsageEvent
                    {
                        VoucherId = voucherId,
                        CustomerId = CustomerId.Value,
                        BranchId = BranchId,
                        OrderId = Id,
                        DiscountApply = discountApply,
                    }
                );
            }
        }

        public void UpdateStatus(OrderStatus status, List<OrderEquipment>? orderEquipment = null)
        {
            if (Status != status)
            {
                Emit(new UpdateStatusOrderEvent() { Order = this });
            }
            switch (status)
            {
                case OrderStatus.InProgress:
                    Status = OrderStatus.InProgress;
                    if (orderEquipment != null)
                    {
                        OrderEquipments = orderEquipment;
                        Emit(new UseEquipmentOrder { OrderEquipments = [.. orderEquipment] });
                    }
                    break;
                case OrderStatus.Processed:
                    Status = OrderStatus.Processed;
                    Emit(new UseEquipmentOrder { OrderEquipments = [.. this.OrderEquipments] });
                    break;
                case OrderStatus.Completed:
                    Status = OrderStatus.Completed;
                    OrderDate = DateTimeOffset.UtcNow;
                    Emit(new EInvoiceEvent() { Order = this });
                    Emit(
                        new CreateFundEvent()
                        {
                            TypeId = "income",
                            ReferenceId = Id,
                            Amount = Total,
                            PaymentMethod = PaymentMethod ?? Enums.PaymentMethod.Cash,
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

                    break;
                case OrderStatus.Cancelled:

                    if (Status == OrderStatus.Completed)
                    {
                        Emit(
                            new CreateFundEvent()
                            {
                                TypeId = "Spend",
                                ReferenceId = Id,
                                Amount = Total,
                                PaymentMethod = PaymentMethod ?? Enums.PaymentMethod.Cash,
                                BranchId = BranchId,
                                ObjectId = CustomerId,
                                BehaviorId = 2,
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
                    Status = OrderStatus.Cancelled;
                    break;
                default:
                    Status = status;
                    break;
            }
        }
    }
}
