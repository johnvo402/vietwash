using Domain.Aggregates.Vouchers;

namespace Application.Feature.Vouchers.Queries.Detail;
public static class GetVoucherDetailMapping
{
    public static GetVoucherDetailResponse ToGetVoucherDetailResponse(this Voucher equipment)
    {
        var response = new GetVoucherDetailResponse();
        response.MappingFrom(equipment);

        return response;
    }
}