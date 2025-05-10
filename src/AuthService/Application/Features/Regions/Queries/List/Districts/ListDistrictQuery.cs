using Application.Features.Common.Projections.Regions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Regions.Queries.List.Districts;

public class ListDistrictQuery
    : QueryParamRequest,
        IRequest<PaginationResponse<DistrictDetailProjection>>;
