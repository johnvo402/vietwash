using Domain.Aggregates.Products;
using System.Linq.Expressions;

namespace Application.Feature.Products.Queries.Detail
{
	public static class GetProductDetailMapping
	{
		public static Expression<Func<Product, GetProductDetailResponse>> Selector() =>
			product => new GetProductDetailResponse
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

		public static GetProductDetailResponse ToCreateUserResponse(this Product product)
		{
			var response = new GetProductDetailResponse();
			response.MappingFrom(product);
			return response;
		}
	}
}
