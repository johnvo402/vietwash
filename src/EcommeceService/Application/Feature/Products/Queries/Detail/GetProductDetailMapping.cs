using Domain.Aggregates.Products;
using System.Linq.Expressions;

namespace Application.Feature.Products.Queries.Detail
{
	public static class GetProductDetailMapping
	{
		public static GetProductDetailResponse ToGetProductDetailResponse(this Product product)
		{
			var response = new GetProductDetailResponse();
			response.MappingFrom(product);
			return response;
		}
	}
}
