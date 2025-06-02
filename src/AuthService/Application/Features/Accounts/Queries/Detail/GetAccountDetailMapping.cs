using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Queries.Detail;

public class GetAccountDetailMapping : Profile
{
    public GetAccountDetailMapping()
    {
        CreateMap<Account, GetAccountDetailResponse>().IncludeBase<Account, AccountDetailProjection>();
    }
}
