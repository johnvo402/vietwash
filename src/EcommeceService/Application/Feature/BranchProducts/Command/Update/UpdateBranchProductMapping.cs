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
				barcode: model.Barcode,
				image: model.Image,
				status: model.Status
			);
			entity.UnitRelations = model.UnitRelations.ToListUnitRelation() ?? [];
		}
	}
}
