using Application.Feature.Common.Projections.Vouchers;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Commands.Create
{
    public static class CreateVoucherMapping
    {
        public static Voucher ToEntity(this VoucherModel model)
        {
            var voucher = new Voucher(
                code: model.Code,
                title: model.Title,
                imgUrl: model.ImgUrl,
                barcode: model.Barcode,
                discountFixed: model.DiscountFixed,
                discountValue: model.DiscountValue,
                totalQuantity: model.TotalQuantity,
                usedQuantity: model.UsedQuantity,
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

            if (hasGroups)
            {
                foreach (var group in model.CustomerGroups.Distinct())
                    voucher.AssignToCustomerGroup(group);
            }

            if (hasCustomers)
            {
                foreach (var customerId in model.CustomerIds.Distinct())
                    voucher.AssignToCustomer(customerId);
            }

            return voucher;
        }
    }
}
