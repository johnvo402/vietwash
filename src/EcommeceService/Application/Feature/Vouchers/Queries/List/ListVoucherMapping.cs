using System.Linq.Expressions;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Queries.List
{
    public class ListVoucherMapping
    {
        public static Expression<Func<Voucher, ListVoucherResponse>> Selector()
        {
            return voucher => new ListVoucherResponse
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Title = voucher.Title,
                ImgUrl = voucher.ImgUrl,
                Barcode = voucher.Barcode,
                DiscountFixed = voucher.DiscountFixed,
                DiscountValue = voucher.DiscountValue,
                TotalQuantity = voucher.TotalQuantity,
                UsedQuantity = voucher.UsedQuantity,
                //VoucherCustomerGroups = voucher.VoucherCustomerGroups,
                StartAt = voucher.StartAt,
                EndAt = voucher.EndAt,
                Status = voucher.Status,
                Description = voucher.Description,
                PublicId = voucher.PublicId,
                CreatedAt = voucher.CreatedAt,
                CreatedBy = voucher.CreatedBy,
                UpdatedAt = voucher.UpdatedAt,
                UpdatedBy = voucher.UpdatedBy,
            };
        }
    }
}
