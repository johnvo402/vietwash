using Application.Feature.Common.Projections.Suppliers;
using Contracts.Utils;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Suppliers.Command.Update
{
    public static class UpdateSupplierMapping
    {
        public static Supplier FromModel(this Supplier supplier, SupplierModel model)
        {
            var code = model.Code;
            if (string.IsNullOrEmpty(code))
            {
                code = Generator.GenerateCode("SP", 6);
            }
            supplier.Update(
                name: model.Name,
                email: model.Email,
                address: model.Address,
                phone: model.Phone,
                description: model.Description,
                status: model.Status,
                disable: null
            );
            supplier.Code = code;

            return supplier;
        }
    }
}
