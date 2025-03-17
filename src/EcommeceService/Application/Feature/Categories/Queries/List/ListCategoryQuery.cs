using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Categories.Queries.List;

public class ListCategoryQuery : QueryParamRequest, IRequest<IEnumerable<ListCategoryResponse>>;
