using Application.Features.Common.Projections.Funds;
using AutoMapper;
using Domain.Aggregates.Funds;


namespace Application.Features.Funds.Queries.Detail
{
    public class GetFundDetailMapping : Profile
    {
        public GetFundDetailMapping()
        {
            CreateMap<Fund, GetFundDetailResponse>();
        }
    }
}
