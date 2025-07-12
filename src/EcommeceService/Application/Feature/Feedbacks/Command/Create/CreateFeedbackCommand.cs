using Application.Feature.Common.Projections.Feedbacks;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Feedbacks.Command.Create;

public class CreateFeedbackCommand : IRequest<Result>
{
    [FromRoute(Name = RouterBase.Id)]
    public long Id { get; set; }

    [FromBody]
    public FeedbackModel FeedbackModel { get; set; }
};
