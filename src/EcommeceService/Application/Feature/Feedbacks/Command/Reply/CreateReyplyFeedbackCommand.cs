using Application.Feature.Common.Projections.Feedbacks;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Feedbacks.Command.Reply
{
    public class CreateReyplyFeedbackCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }

        [FromBody]
        public ReplyFeedbackModel ReplyFeedback { get; set; }
    };
}
