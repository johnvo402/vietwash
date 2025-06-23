using AutoMapper;
using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Queries.List
{
    public class ListFundMapping : Profile
    {
        public ListFundMapping()
        {
            CreateMap<Fund, ListFundResponse>();
        }
    }
}
