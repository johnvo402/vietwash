using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.EquipmentActivities.Queries.List;

public class ListEquipmentActivityQuery : QueryParamRequest,
		IRequest<Result<PaginationResponse<ListEquipmentActivityResponse>>>;
