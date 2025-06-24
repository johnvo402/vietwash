using System.Linq.Expressions;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Suppliers.Query.Detail
{
    public static class GetSupplierDetailMapping
    {
        public static Expression<Func<Supplier, GetSupplierDetailResponse>> Selector() =>
            supplier => new GetSupplierDetailResponse
            {
                Id = supplier.Id,
                PublicId = supplier.PublicId,
                CreatedAt = supplier.CreatedAt,
                CreatedBy = supplier.CreatedBy,
                UpdatedAt = supplier.UpdatedAt,
                UpdatedBy = supplier.UpdatedBy,

                Name = supplier.Name,
                Code = supplier.Code,
                Email = supplier.Email,
                Address = supplier.Address,
                Phone = supplier.Phone,
                Description = supplier.Description,
                Status = supplier.Status,
            };

        public static GetSupplierDetailResponse ToCreateUserResponse(this Supplier supplier)
        {
            var response = new GetSupplierDetailResponse();
            response.MappingFrom(supplier);
            return response;
        }
    }
}
