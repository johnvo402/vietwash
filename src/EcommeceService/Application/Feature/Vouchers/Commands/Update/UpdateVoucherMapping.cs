using Application.Feature.Common.Projections.Vouchers;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Commands.Update
{
    public static class UpdateVoucherMapping
    {
        public static void FromUpdateModel(this Voucher entity, VoucherModel model, string barcode)
        {
            entity.Update(
                code: model.Code,
                title: model.Title,
                imgUrl: model.ImgUrl,
                barcode: barcode,
                discountFixed: model.DiscountFixed,
                discountValue: model.DiscountValue,
                // totalQuantity: model.TotalQuantity,
                // usedQuantity: model.UsedQuantity,
                startAt: model.StartAt,
                endAt: model.EndAt,
                status: model.Status,
                description: model.Description
            );

            // entity.UpdateCustomerGroups(model.CustomerGroups);
        }
    }
}
