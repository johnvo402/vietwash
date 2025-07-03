using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Equipments.Queries.Detail;

public class GetEquipmentDetailQuery : IRequest<Result<GetEquipmentDetailResponse>>
{
	[FromRoute(Name = RouterBase.Id)]
	public long EquipmentId { get; set; }
}
