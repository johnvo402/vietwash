using Application.Feature.Common.Mapping.Units;
using Application.Feature.Common.Projections.BranchProducts;
using Domain.Aggregates.Products;

namespace Application.Feature.BranchProducts.Command.Update
{
    public static class UpdateBranchProductMapping
    {
        public static void FromUpdateModel(this BranchProduct entity, BranchProductModel model)
        {
            entity.Update(
                branchId: model.BranchId,
                name: model.Name,
                description: model.Description,
                sku: model.Sku,
                image: model.Image,
                status: model.Status,
                capitalPrice: model.CapitalPrice,
                categoryId: model.CategoryId
            );
            entity.UnitRelations = model.UnitRelations.ToListUnitRelation() ?? [];
        }
    }
}
