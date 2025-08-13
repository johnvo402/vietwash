using System.Linq.Expressions;
using Application.Feature.Common.Mapping.Categories;
using Application.Feature.Common.Mapping.Units;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Products;

namespace Application.Feature.BranchProducts.Queries.List
{
    public static class ListBranchProductMapping
    {
        public static Expression<Func<BranchProduct, ListBranchProductResponse>> Selector() =>
            products => new ListBranchProductResponse
            {
                Id = products.Id,
                PublicId = products.PublicId,

                Name = products.Name,
                Image = products.Image,
                Status = products.Status,
                CategoryId = products.CategoryId,
                Description = products.Description,
                BranchId = products.BranchId,
                Sku = products.Sku,
                CapitalPrice = products.CapitalPrice,
                StockQuantity = products
                    .ProductSupplyings.Where(x =>
                        x.InventoryDocument.Status == InventoryStatus.Completed
                    )
                    .Sum(i => i.Quantity * i.UnitRelation.Multiple),
                Category = products.Category.ToCategoryService(),
                UnitRelations = products
                    .UnitRelations.Select(x => x.ToUnitRelationProjectionResponse())
                    .ToList(),
            };
    }
}
