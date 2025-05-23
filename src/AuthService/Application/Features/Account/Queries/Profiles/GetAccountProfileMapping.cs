using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Queries.Profiles;

public class GetAccountProfileMapping : Profile
{
    public GetAccountProfileMapping()
    {
        CreateMap<Account, GetAccountProfileResponse>()
            .IncludeBase<Account, AccountDetailProjection>();
    }
}
