

using Application.Features.Common.Projections.Funds;
using AutoMapper;
using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundMapping : Profile
    {
        public UpdateFundMapping()
        {
            CreateMap<UpdateFundCommand, Fund>();

            CreateMap<UpdateFundModel, Fund>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
