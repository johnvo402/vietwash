using Application.Feature.Common.Projections.Suppliers;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Suppliers.Command.Update;

public class UpdateSupplierCommand : IRequest<Result>
{
    [FromRoute(Name = RouterBase.Id)]
    public long SupplierId { get; set; } = default!;

    [FromBody]
    public SupplierModel Supplier { get; set; } = default!;
}
