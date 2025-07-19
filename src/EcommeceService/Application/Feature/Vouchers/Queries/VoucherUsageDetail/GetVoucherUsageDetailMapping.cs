using Application.Feature.Equipments.Queries.Detail;


namespace Application.Feature.Vouchers.Queries.VoucherUsageDetail;

public static class GetVoucherUsageDetailMapping
{
    public static GetVoucherUsageDetailResponse ToGetVoucherUsageDetailResponse(this Domain.Aggregates.Vouchers.VoucherUsage voucherUsage)
    {
        var response = new GetVoucherUsageDetailResponse();
        response.MappingFrom(voucherUsage);
        return response;
    }
}
