using Contracts.Application.Common;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Vouchers;
using System;

namespace Application.Feature.Common.Projections.Vouchers
{
    public class VoucherUsageProjection : BaseResponse
    {
        public long VoucherId { get; set; }
        public long CustomerId { get; set; }
        public long OrderId { get; set; }
        public decimal DiscountApply { get; set; }

        public virtual void MappingFrom(VoucherUsage voucherUsage)
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
        }
    }
}
