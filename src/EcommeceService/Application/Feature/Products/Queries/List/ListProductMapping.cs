using Domain.Aggregates.Products;
using System.Linq.Expressions;

namespace Application.Feature.Products.Queries.List
{
	public static class ListProductMapping
	{
		public static Expression<Func<Product, ListProductResponse>> Selector() =>
			product => new ListProductResponse
			{
				Id = product.Id,
				PublicId = product.PublicId,
				CreatedAt = product.CreatedAt,
				CreatedBy = product.CreatedBy,
				UpdatedAt = product.UpdatedAt,
				UpdatedBy = product.UpdatedBy,

				Name = product.Name,
				Description = product.Description,
				Sku = product.Sku,
				Status = product.Status,
				Barcode = product.Barcode,
				RecommendedPrice = product.RecommendedPrice,
			};
	}
}
