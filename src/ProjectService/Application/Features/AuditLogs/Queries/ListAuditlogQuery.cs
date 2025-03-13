using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.UseCases.AuditLogs.Queries;

public class ListAuditlogQuery() : QueryParamRequest, IRequest<PaginationResponse<ListAuditlogResponse>>;
