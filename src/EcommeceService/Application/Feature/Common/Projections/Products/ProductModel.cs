using Domain.Aggregates.Products.Enums;

namespace Application.Feature.Common.Projections.Products
{
	public class ProductModel
	{
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public string Sku { get; set; } = default!;
		public ProductStatus Status { get; set; } = ProductStatus.Active;
		public string Barcode { get; set; } = default!;
		public decimal RecommendedPrice { get; set; } = default!;
	}
}
