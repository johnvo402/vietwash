using AutoMapper;
using Domain.Aggregates.Funds;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorMapping : Profile
    {
        public CreateFundBehaviorMapping()
        {
            CreateMap<CreateFundBehaviorCommand, FundBehavior>();
        }

    }
}
