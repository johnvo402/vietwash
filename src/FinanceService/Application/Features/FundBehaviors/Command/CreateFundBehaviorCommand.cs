using Application.Features.Common.Projections.FundBehaviors;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorCommand : CreateFundBehaviorModel, IRequest<Result>;
}
