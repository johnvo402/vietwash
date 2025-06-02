using Application.Features.Common.Projections.Accounts;
using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountMapping : Profile
{
    public ListAccountMapping()
    {
        CreateMap<Account, ListAccountResponse>().IncludeBase<Account, AccountProjection>();
        // .ForMember(
        //     dest => dest.Age,
        //     opt =>
        //         opt.MapFrom(src =>
        //             src.DayOfBirth == null
        //                 ? 0
        //                 : DateTimeOffset.UtcNow.Year - src.DayOfBirth.Value.Year
        //         )
        // );
    }
}
