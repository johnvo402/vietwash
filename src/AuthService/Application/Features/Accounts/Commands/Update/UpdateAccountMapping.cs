using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountMapping : Profile
{
    public UpdateAccountMapping()
    {
        CreateMap<UpdateAccount, Account>().IncludeBase<AccountModel, Account>();
        CreateMap<Account, UpdateAccountResponse>().IncludeBase<Account, AccountDetailProjection>();
    }
}
