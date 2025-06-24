using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Suppliers.Query.List
{
    public class ListSupplierQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListSupplierResponse>>>;
}
