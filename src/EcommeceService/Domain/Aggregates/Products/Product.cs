using Domain.Aggregates.Products.Enums;
using Shared.Kernel.Common;
using Mediator;
using Ardalis.GuardClauses;

namespace Domain.Aggregates.Products
{
	public class Product : AggregateRoot
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Sku { get; set; }
		public ProductStatus Status { get; set; }
		public string Barcode { get; set; }
		public decimal RecommendedPrice { get; set; }
		public bool Disable { get; set; } = false;

		public Product() { }

		public Product(string name, string sku, ProductStatus status, string? description = null, string barcode = null, decimal? recommendedPrice = null)
		{
			Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
			Sku = Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
			Status = Guard.Against.EnumOutOfRange(status, nameof(status));
			Description = description?.Trim();
			Barcode = Guard.Against.NullOrWhiteSpace(barcode, nameof(barcode));
			RecommendedPrice = recommendedPrice ?? 0;
		}

		public void Update(
		   string? name = null,
		   string? sku = null,
		   ProductStatus? status = null,
		   string? description = null,
		   string? barcode = null,
		   decimal? recommendedPrice = null,
		   bool? disable = null
	   )
		{
			if (!string.IsNullOrWhiteSpace(name))
				Name = name.Trim();

			if (!string.IsNullOrWhiteSpace(sku))
				Sku = sku.Trim();

			if (status.HasValue)
				Status = status.Value;

			if (description != null)
				Description = description.Trim();

			if (!string.IsNullOrWhiteSpace(sku))
				Barcode = barcode.Trim();

			if (recommendedPrice.HasValue)
				RecommendedPrice = recommendedPrice.Value;

			if (disable.HasValue)
				Disable = disable.Value;
		}

		protected override bool TryApplyDomainEvent(INotification domainEvent)
		{
			throw new NotImplementedException();
		}
	}
}
