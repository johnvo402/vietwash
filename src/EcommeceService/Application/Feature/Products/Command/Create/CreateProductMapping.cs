using Application.Feature.Common.Projections.Products;
using Contracts.Extensions;
using Domain.Aggregates.Products;

namespace Application.Feature.Products.Command.Create
{
	public static class CreateProductMapping
	{
		public static Product ToEntity(this ProductModel model)
		{
			return new Product(
			   name: model.Name.Trim(),
			   description: model.Description?.Trim(),
			   sku: model.Sku.Trim(),
			   barcode: model.Barcode.Trim(),
			   image: model.Image,
			   status: model.Status,
			   recommendedPrice: model.RecommendedPrice
		   )
			{
				ProductBranches = model.ProductBranches.ToListMapping(branch => new ProductBranch
				{
					BranchId = branch.BranchId,
					Description = model.Description?.Trim(),
					Sku = model.Sku.Trim(),
					Barcode = model.Barcode.Trim(),
					Status = model.Status,
					Image = model.Image
				})
			};
		}
	}
}
