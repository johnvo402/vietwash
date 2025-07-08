using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Feedbacks.Queries.ListByService;

public class ListFeedbackByServiceQuery : IRequest<Result<IEnumerable<ListFeedbackByServiceResponse>>>
{
	[FromRoute(Name = RouterBase.Id)]
	public long ServiceId { get; set; }
}
