using Domain.Aggregates.Funds;

namespace Application.Features.FundBehaviors.Command
{
    public static class CreateFundBehaviorMapping
    {
        public static FundBehavior ToEntity(this CreateFundBehaviorCommand command)
        {
            return new FundBehavior(command.Name, command.Type);
        }
    }
}
