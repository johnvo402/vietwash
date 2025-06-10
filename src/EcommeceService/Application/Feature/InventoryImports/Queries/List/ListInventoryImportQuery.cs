using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;


namespace Application.Feature.InventoryImports.Queries.List
{
    public class ListInventoryImportQuery : QueryParamRequest, IRequest<PaginationResponse<ListInventoryImportResponse>>;
}
