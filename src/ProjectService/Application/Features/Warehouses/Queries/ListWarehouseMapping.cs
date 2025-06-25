using System.Linq.Expressions;
using Application.Features.Common.Projections.Warehouses;
using Domain.Aggregates.Warehouses;

namespace Application.Features.Warehouses.Queries
{
    public static class WarehouseMapping
    {
        public static Expression<Func<Warehouse, ListWarehouseResponse>> Selector()
        {
            return w => new ListWarehouseResponse
            {
                Id = w.Id,
                CreatedAt = w.CreatedAt,
                CreatedBy = w.CreatedBy,
                UpdatedAt = w.UpdatedAt,
                UpdatedBy = w.UpdatedBy,

                Name = w.Name,
                Code = w.Code,
                Description = w.Description,
                ReorderLevel = w.ReorderLevel,
                BranchId = w.BranchId,
                Status = w.Status,
            };
        }
    }
}
