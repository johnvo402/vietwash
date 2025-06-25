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
				description: model.Description,
				sku: model.Sku,
				barcode: model.Barcode,
				image: model.Image,
				status: model.Status,
				recommendedPrice: model.RecommendedPrice,
				disable: null
			);

			return product;
		}
	}
}
