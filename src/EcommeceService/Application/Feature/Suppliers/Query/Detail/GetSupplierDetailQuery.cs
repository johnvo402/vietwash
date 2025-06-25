using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Suppliers.Query.Detail
{
    public record GetSupplierDetailQuery([FromRoute(Name = RouterBase.Id)] long SupplierId)
        : IRequest<Result<GetSupplierDetailResponse>>;
}
