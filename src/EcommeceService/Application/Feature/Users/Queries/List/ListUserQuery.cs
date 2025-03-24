
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Users.Queries.List;

public class ListUserQuery : QueryParamRequest, IRequest<PaginationResponse<ListUserResponse>>;
