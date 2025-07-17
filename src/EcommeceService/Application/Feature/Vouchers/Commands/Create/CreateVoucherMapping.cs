using Application.Feature.Common.Projections.Vouchers;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Commands.Create
{
    public static class CreateVoucherMapping
    {
        public static Voucher ToEntity(this VoucherModel model)
        {
            return new Voucher(
                code: model.Code,
            title: model.Title,
          imgUrl: model.ImgUrl,
    barcode: model.Barcode,
         discountFixed: model.DiscountFixed,
         discountValue: model.DiscountValue,
        totalQuantity: model.TotalQuantity,
         usedQuantity: model.UsedQuantity,
  description: model.Description,
        startAt: model.StartAt,
         endAt: model.EndAt,
       status: model.Status,
       customerGroups: model.CustomerGroups
);
        }
    }
}
