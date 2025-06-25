using Application.Feature.Common.Projections.Suppliers;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Suppliers.Command.Update
{
    public static class UpdateSupplierMapping
    {
        public static Supplier FromModel(this Supplier supplier, SupplierModel model)
        {
            supplier.Update(
                name: model.Name,
                email: model.Email,
                address: model.Address,
                phone: model.Phone,
                description: model.Description,
                status: model.Status,
                disable: null
            );

            return supplier;
        }
    }
}
