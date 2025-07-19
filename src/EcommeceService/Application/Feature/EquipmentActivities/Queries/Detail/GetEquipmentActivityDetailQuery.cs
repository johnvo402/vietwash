using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.EquipmentActivities.Queries.Detail
{
	public class GetEquipmentActivityDetailQuery : IRequest<Result<GetEquipmentActivityDetailResponse>>

	{
		[FromRoute(Name = RouterBase.Id)]
		public long EquipmentActivityId { get; set; }
	}
}
