using Application.Feature.Common.Projections.Vouchers;
using Contracts.Utils;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Commands.Create
{
    public static class CreateVoucherMapping
    {
        public static Voucher ToEntity(this VoucherModel model, string barcode)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                model.Code = Generator.GenerateRandomString(9);
            }

            var voucher = new Voucher(
                code: model.Code,
                title: model.Title,
                imgUrl: model.ImgUrl,
                barcode: barcode,
                discountFixed: model.DiscountFixed,
                discountValue: model.DiscountValue,
                startAt: model.StartAt,
                endAt: model.EndAt,
                status: model.Status,
                description: model.Description
            );

            bool hasGroups = model.CustomerGroups?.Any() == true;
            bool hasCustomers = model.CustomerIds?.Any() == true;

            if (hasGroups && hasCustomers)
                throw new InvalidOperationException(
                    "Voucher cannot be assigned to both groups and individual customers at the same time."
                );

            return voucher;
        }
    }
}
