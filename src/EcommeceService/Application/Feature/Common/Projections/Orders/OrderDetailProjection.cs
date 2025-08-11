using Application.Common.Security;
using Application.Feature.Common.Projections.Tariffs;
using Application.Features.Common.Mapping.Users;
using Contracts.Extensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderDetailProjection : OrderProjection
    {
        public string? Note { get; set; }
        public long? StaffId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? QrCode { get; set; }
        public string? VoucherCode { get; set; }
        public int Vat { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Point { get; set; }
        public long? TariffId { get; set; }
        public TariffByBranchProjection? Tariff { get; set; }
        public ICollection<OrderItemProjection> OrderItems { get; set; } = [];
        public ICollection<OrderEquipmentProjection> OrderEquipments { get; set; } = [];

        public virtual void MappingFrom(Order order)
        {
            Id = order.Id;
            PublicId = order.PublicId;
            CreatedAt = order.CreatedAt;
            CreatedBy = order.CreatedBy;
            UpdatedAt = order.UpdatedAt;
            UpdatedBy = order.UpdatedBy;

            Code = order.Code;
            Amount = order.Amount;
            Vat = order.Vat;
            VatAmount = order.VatAmount;
            Total = order.Total;
            DiscountFixed = order.DiscountFixed;
            DiscountValue = order.DiscountValue;
            CustomerId = order.CustomerId;
            Note = order.Note;
            OrderDate = order.OrderDate;
            DeliveryTime = order.DeliveryTime;
            Status = order.Status;
            BranchId = order.BranchId;
            QrCode = order.CodeConfirm;
            PaymentMethod = order.PaymentMethod;
            VoucherCode = order.VoucherCode;
            Point = order.Point;
            TariffId = order.TariffId;
            Tariff =
                order?.Tariff != null
                    ? new TariffByBranchProjection
                    {
                        Id = (long)order.Tariff?.Id!,
                        Name = order.Tariff?.Name!,
                    }
                    : null;
            OrderItems =
                order
                    ?.OrderItems.ToListMapping(item => new OrderItemProjection
                    {
                        Id = item.Id,
                        OrderId = item.OrderId,
                        ServiceId = item.ServiceId,
                        ServiceImage = item.Service.Image,
                        UnitRelationId = item.UnitRelationId,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        UnitRelationName = item.UnitRelationName,
                        ProcessingTime = item.ProcessingTime,
                        ServiceName = item.ServiceName,
                        UnitPrice = item.UnitPrice,
                    })
                    .ToList() ?? [];
            OrderEquipments =
                order
                    ?.OrderEquipments.ToListMapping(item => new OrderEquipmentProjection
                    {
                        Code = item.Equipment.Code,
                        Image = item.Equipment.Image,
                        EquipmentName = item.Equipment.Name,
                    })
                    .ToList() ?? [];

            Customer = order?.Customer?.UserDTOResponse() ?? null;
            Staff = order?.Staff?.UserDTOResponse() ?? null;
            TotalProcessTime = order?.OrderItems.Sum(x => x.ProcessingTime) ?? 0;
        }
    }

    public class OrderEquipmentProjection
    {
        public string Code { get; set; }

        [File]
        public string? Image { get; set; }

        public string EquipmentName { get; set; }
    }

    public class OrderItemProjection
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ServiceId { get; set; }

        [File]
        public string? ServiceImage { get; set; }
        public long UnitRelationId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? UnitRelationName { get; set; }
        public decimal ProcessingTime { get; set; }
        public string? ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
