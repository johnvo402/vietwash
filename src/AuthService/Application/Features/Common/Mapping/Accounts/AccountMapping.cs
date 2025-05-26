using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Common.Mapping.Accounts;

public class AccountMapping : Profile
{
    public AccountMapping()
    {
        CreateMap<Account, AccountProjection>();

        CreateMap<Account, AccountDetailProjection>();
        CreateMap<AccountModel, Account>();
        CreateMap<AccountContact, AccountContactProjection>();
        CreateMap<AccountContactProjection, AccountContact>();
    }
}
