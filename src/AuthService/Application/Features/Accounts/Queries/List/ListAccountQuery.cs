using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountQuery : QueryParamRequest, IRequest<PaginationResponse<ListAccountResponse>>;
