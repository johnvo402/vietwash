using Application.Feature.Common.Projections.Vouchers;
using Domain.Aggregates.Vouchers;

public class VoucherUsageDetailProjection : VoucherUsageProjection
{
    public override void MappingFrom(VoucherUsage voucherUsage)
    {
        base.MappingFrom(voucherUsage);
    }
}
