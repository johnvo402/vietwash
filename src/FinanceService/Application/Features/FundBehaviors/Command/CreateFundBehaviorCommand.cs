using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.FundBehaviors;
using AutoMapper;
using Domain.Aggregates.Funds;
using Mediator;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorCommand : CreateFundBehaviorModel, IRequest
    {
       
    }
}
