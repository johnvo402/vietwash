using Application.Feature.Common.Projections.Feedbacks;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Feedbacks.Command.Update;

public class UpdateFeedbackCommand : IRequest<Result>
{
    [FromRoute(Name = RouterBase.Id)]
    public long FeedbackId { get; set; } = default!;

    [FromBody]
    public UpdateFeedbackModel Feedback { get; set; } = default!;
}
