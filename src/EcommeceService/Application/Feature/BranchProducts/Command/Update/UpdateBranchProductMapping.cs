using Application.Feature.Common.Projections.BranchProducts;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;


namespace Application.Feature.BranchProducts.Command.Update
{
	public static class UpdateBranchProductMapping
	{
		public static void FromUpdateModel(this BranchProduct entity, UpdateBranchProductModel model)
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
			if (model.UnitRelations?.Any() == true)
			{
				foreach (var item in model.UnitRelations)
				{
					var existingUnit = entity.UnitRelations.FirstOrDefault(u => u.Id == item.Id);
					if (existingUnit != null)
					{
						existingUnit.Name = item.Name;
						existingUnit.BaseUnit = item.BaseUnit;
						existingUnit.Price = item.Price;
						existingUnit.Multiple = item.Multiple;
						existingUnit.ProcessingTime = item.ProcessingTime;
						existingUnit.Status = item.Status;
					}
					else
					{
						entity.UnitRelations.Add(new UnitRelation
						{
							Name = item.Name,
							BaseUnit = item.BaseUnit,
							Price = item.Price,
							Multiple = item.Multiple,
							ProcessingTime = item.ProcessingTime,
							Status = item.Status,
							BranchProductId = entity.Id
						});
					}

				}

			}
		}
	}
}
