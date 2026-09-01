using System.Linq.Expressions;
using Domain.Aggregates.Enums;

namespace Domain.Aggregates.Vouchers.Specifications;

public static class VoucherEligibility
{
    public static Expression<Func<Voucher, bool>> ForCustomer(
        string code,
        long customerId,
        DateTimeOffset now
    ) =>
        voucher =>
            voucher.Code == code
            && voucher.Status == ActivationStatus.Active
            && (!voucher.StartAt.HasValue || voucher.StartAt.Value <= now)
            && (!voucher.EndAt.HasValue || voucher.EndAt.Value >= now)
            && voucher.VoucherCustomers.Any(x => x.CustomerId == customerId && !x.IsUsed);
}
