using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Queries.Detail
{
    public static class GetFundDetailMapping
    {
        public static GetFundDetailResponse ToGetFundDetailResponse(this Fund fund)
        {
            var response = new GetFundDetailResponse();
            response.MappingFrom(fund);
            return response;
        }
    }
}
