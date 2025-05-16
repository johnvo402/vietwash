using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;
namespace Application.Features.Accounts.Commands.Create;

public class CreateAccountMapping : Profile
{
    public CreateAccountMapping()
    {
        

        CreateMap<CreateAccountCommand, Account>()
            .AfterMap(
                (src, dest) =>
                {
                    dest.SetPassword(HashPassword(src.Password));
                }
            );

        CreateMap<Account, CreateAccountResponse>().IncludeBase<Account, AccountDetailProjection>();
        CreateMap<Account, CreateAccountCommand>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());
    }
}
