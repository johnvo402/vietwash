using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountMapping : Profile
{
    public UpdateAccountMapping()
    {
        CreateMap<UpdateAccount, Account>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());
        CreateMap<Account, UpdateAccountResponse>().IncludeBase<Account, AccountDetailProjection>();
    }
}
