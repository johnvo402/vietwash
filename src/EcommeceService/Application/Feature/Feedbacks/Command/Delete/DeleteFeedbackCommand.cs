using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Feedbacks.Command.Delete
{
    public record DeleteFeedbackCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long FeedbackId { get; set; } = default!;
    }
}
