using Application.Features.Accounts.Queries.Detail;
using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Login;

public class LoginUserMapping : Profile
{
    public LoginUserMapping()
    {
        CreateMap<Account, GetAccountDetailResponse>().IncludeBase<Account, AccountDetailProjection>();

    }
}
