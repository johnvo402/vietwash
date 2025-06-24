using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Suppliers.Query.Detail
{
    public record GetSupplierDetailQuery(long SupplierId)
        : IRequest<Result<GetSupplierDetailResponse>>;
}
