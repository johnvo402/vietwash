using Domain.Aggregates.Enums;
using Domain.Aggregates.Products.Enums;

namespace Application.Feature.Common.Projections.Products
{
	public class ProductModel
	{
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public string Sku { get; set; } = default!;
		public string Barcode { get; set; } = default!;
		public string? Image { get; set; }
		public ActivationStatus Status { get; set; } = ActivationStatus.Active;
		public decimal RecommendedPrice { get; set; } = default!;
		public List<ProductBranchModel> ProductBranches { get; set; } = [];

	}
}
