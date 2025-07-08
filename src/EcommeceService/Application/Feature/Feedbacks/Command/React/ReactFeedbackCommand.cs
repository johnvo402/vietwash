using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Feedbacks.Command.React
{
	public class ReactFeedbackCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long FeedbackId { get; set; }
		[FromBody]
		public bool IsLike { get; set; } 
	}
}
