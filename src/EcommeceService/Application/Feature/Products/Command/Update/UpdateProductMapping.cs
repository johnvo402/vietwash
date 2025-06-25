using Application.Feature.Common.Projections.Products;
using Domain.Aggregates.Products;


namespace Application.Feature.Products.Command.Update
{
	public static class UpdateProductMapping
	{
		public static Product FromModel(this Product product, ProductModel model)
		{
			product.Update(
				name: model.Name,
				sku: model.Sku,
				status: model.Status,
				description: model.Description,
				barcode: model.Barcode,
				recommendedPrice: model.RecommendedPrice,
				disable: null
			);

			return product;
		}
	}
}
