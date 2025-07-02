using Application.Feature.Common.Mapping.Units;
using Application.Feature.Common.Projections.BranchProducts;
using Domain.Aggregates.Products;

namespace Application.Feature.BranchProducts.Command.Create
{
	public static class CreateBranchProductMapping
	{
		public static BranchProduct ToEntity(this BranchProductModel model)
		{
			var result = new BranchProduct(
				branchId: model.BranchId,
				name: model.Name,
				description: model.Description,
				sku: model.Sku,
				barcode: model.Barcode,
				image: model.Image,
				status: model.Status
			);
			result.UnitRelations = model.UnitRelations.ToListUnitRelation() ?? [];
			return result;
		}
	}
}
