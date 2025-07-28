using System.Linq.Expressions;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Suppliers.Query.List
{
    public static class ListSupplierMapping
    {
        public static Expression<Func<Supplier, ListSupplierResponse>> Selector() =>
            supplier => new ListSupplierResponse
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
                TotalInventory = supplier.ProductSupplyings.Sum(ps => ps.Quantity * ps.Price),
            };
    }
}
