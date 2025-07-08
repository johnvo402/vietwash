using Application.Feature.Common.Projections.Feedbacks;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Feedbacks.Command.Reply
{
	public class CreateReyplyFeedbackCommand : ReplyFeedbackModel, IRequest<Result>;
}
