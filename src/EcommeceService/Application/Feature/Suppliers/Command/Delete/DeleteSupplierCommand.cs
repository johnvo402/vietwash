using Mediator;

namespace Application.Feature.Suppliers.Command.Delete
{
    public record DeleteSupplierCommand(long SupplierId) : IRequest;
}
