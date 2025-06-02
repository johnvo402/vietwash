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
        CreateMap<AccountModel, Account>()
        .ForMember(dest => dest.BirthDay, opt =>
        opt.MapFrom(src => src.BirthDay.HasValue
            ? DateOnly.FromDateTime(src.BirthDay.Value)
            : (DateOnly?)null));
        CreateMap<AccountContact, AccountContactProjection>();
        CreateMap<AccountContactProjection, AccountContact>();
        CreateMap<BranchAccountProjection, BranchAccount>();
        CreateMap<BranchAccount, BranchAccountProjection>();
        CreateMap<BranchAccountModel, BranchAccount>();
    }
}
