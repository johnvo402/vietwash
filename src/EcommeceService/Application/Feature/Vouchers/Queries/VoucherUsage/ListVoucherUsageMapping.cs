using System.Linq.Expressions;

namespace Application.Feature.Vouchers.Queries.VoucherUsage
{
    public class ListVoucherUsageMapping
    {
        public static Expression<Func<Domain.Aggregates.Vouchers.VoucherUsage, ListVoucherUsageResponse>> Selector()
        {
            return voucherUsage => new ListVoucherUsageResponse
            {
                Id = voucherUsage.Id,
                CustomerId = voucherUsage.CustomerId,
                VoucherId = voucherUsage.VoucherId,
                PublicId = voucherUsage.PublicId,
                DiscountApply=voucherUsage.DiscountApply,
                OrderId = voucherUsage.OrderId,
                CreatedAt = voucherUsage.CreatedAt,
                CreatedBy = voucherUsage.CreatedBy,
                UpdatedAt = voucherUsage.UpdatedAt,
                UpdatedBy = voucherUsage.UpdatedBy,
            };
        }
    }
}
