using Application.Feature.Common.Projections.Vouchers;

namespace Application.Feature.Vouchers.Queries.VoucherUsageDetail
{
    public class GetVoucherUsageDetailResponse : VoucherUsageProjection
    {
        public OrderShortInfo? Order { get; set; }

        public virtual void MappingFrom(Domain.Aggregates.Vouchers.VoucherUsage voucherUsage)
        {
            Id = voucherUsage.Id;
            VoucherId = voucherUsage.VoucherId;
            CustomerId = voucherUsage.CustomerId;
            OrderId = voucherUsage.OrderId;
            DiscountApply = voucherUsage.DiscountApply;

            PublicId = voucherUsage.PublicId;
            CreatedAt = voucherUsage.CreatedAt;
            CreatedBy = voucherUsage.CreatedBy;
            UpdatedAt = voucherUsage.UpdatedAt;
            UpdatedBy = voucherUsage.UpdatedBy;

            if (voucherUsage.Order is not null)
            {
                var order = voucherUsage.Order;
                Order = new OrderShortInfo
                {
                    Id = order.Id,
                    Code = order.Code,
                    Total = order.Total,
                    Status = order.Status,
                    OrderDate = order.OrderDate,
                    CustomerName = order.Customer?.DisplayName,
                };
            }
        }
    }

    public class OrderShortInfo
    {
        public long Id { get; set; }
        public string Code { get; set; } = default!;
        public decimal Total { get; set; }
        public Domain.Aggregates.Orders.Enums.OrderStatus Status { get; set; }
        public DateTimeOffset? OrderDate { get; set; }
        public string? CustomerName { get; set; }
    }
}
