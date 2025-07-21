using Application.Feature.Equipments.Queries.List;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Equipments.Queries.Listl;

public class ListEquipmentQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListEquipmentResponse>>>;
