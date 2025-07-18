using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Queries.Detail;

public static class GetVoucherDetailMapping
{
    public static GetVoucherDetailResponse ToGetVoucherDetailResponse(this Voucher voucher)
    {
        var response = new GetVoucherDetailResponse();
        response.MappingFrom(voucher);

        return response;
    }
}
