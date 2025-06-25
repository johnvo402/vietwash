using Application.Feature.Common.Projections.Suppliers;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Suppliers.Enum;

namespace Application.Feature.Suppliers.Command.Create
{
    public static class CreateSupplierMapping
    {
        public static Supplier ToEntity(this SupplierModel model)
        {
            return new Supplier(
                name: model.Name,
                code: model.Code ?? string.Empty,
                status: model.Status ?? SupplierStatus.Active,
                email: model.Email,
                address: model.Address,
                phone: model.Phone,
                description: model.Description
            );
        }
    }
}
