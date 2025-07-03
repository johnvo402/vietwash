using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.InventoryDocuments.Queries.List
{
    public class ListInventoryDocumentQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListInventoryDocumentResponse>>>;
}
