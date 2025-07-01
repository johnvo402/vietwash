using Application.Feature.Common.Projections.BranchProducts;
using Contracts.Extensions;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;

namespace Application.Feature.BranchProducts.Command.Create
{
	public static class CreateBranchProductMapping
	{
		public static BranchProduct ToEntity(this BranchProductModel model)
		{
			return new BranchProduct(
				branchId: model.BranchId,
				name: model.Name,
				description: model.Description,
				sku: model.Sku,
				barcode: model.Barcode,
				image: model.Image,
				status: model.Status
			)
			{
				UnitRelations = model.UnitRelations.ToListMapping(x => new UnitRelation
				{
					Name = x.Name,
					BaseUnit = x.BaseUnit,
					Price = x.Price,
					Multiple = x.Multiple,
					ProcessingTime = x.ProcessingTime,
					Status = x.Status
				})
			};
		}
	}
}
