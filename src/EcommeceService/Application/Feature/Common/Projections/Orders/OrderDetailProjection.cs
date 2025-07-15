using Application.Common.Security;
using Application.Features.Common.Mapping.Users;
using Contracts.Extensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class OrderDetailProjection : OrderProjection
    {
        public string? Note { get; set; }
        public string? Receipt { get; set; }
        public long? StaffId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? BarcodeConfirm { get; set; }
        public ICollection<OrderItemProjection> OrderItems { get; set; } = [];

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
            Total = order.Total;
            DiscountFixed = order.DiscountFixed;
            DiscountValue = order.DiscountValue;
            CustomerId = order.CustomerId;
            Note = order.Note;
            OrderDate = order.OrderDate;
            DeliveryTime = order.DeliveryTime;
            Status = order.Status;
            BranchId = order.BranchId;
            Receipt = order.Receipt;
            BarcodeConfirm = order.BarcodeConfirm;
            PaymentMethod = order.PaymentMethod;
            OrderItems = order
                .OrderItems.ToListMapping(item => new OrderItemProjection
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
                .ToList();

            Customer = order.Customer?.UserDTOResponse() ?? null;
            Staff = order.Staff?.UserDTOResponse() ?? null;
        }
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
