using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Services.Queries.List;

public class ListServiceQuery : QueryParamRequest, IRequest<PaginationResponse<ListServiceResponse>>;
