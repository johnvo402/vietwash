using Application.Feature.Common.Projections.Suppliers;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Suppliers.Command.Create
{
    public class CreateSupplierCommand : SupplierModel, IRequest<Result>;
}
