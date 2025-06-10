using Application.Feature.Common.Projections.Suppliers;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Suppliers.Command.Update;

public class UpdateSupplierCommand : IRequest<UpdateSupplierResponse>
{
    [FromRoute(Name = RouterBase.Id)]
    public long SupplierId { get; set; } = default!;
    [FromBody]
    public SupplierUpdateModel Body { get; set; } = default!;
}
