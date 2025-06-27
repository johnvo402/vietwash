using Ardalis.GuardClauses;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Services;
using Shared.Kernel.Common;


namespace Domain.Aggregates.Products
{
	public class ProductBranch : BaseEntity<long>
	{
		public long BranchId { get; set; } = default!;
		public long ProductId { get; set; } = default!;
		public string? Description { get; set; }
		public string? Sku { get; set; }
		public ActivationStatus Status { get; set; }
		public string? Barcode { get; set; }
		public string? Image { get; set; }
		public Product Product { get; set; } = default!;

		//public ProductBranch(
		//	long branchId,
		//	string? description,
		//	string sku,
		//	string barcode,
		//	ActivationStatus status,
		//	string? image
		//)
		//{
		//	branchId = Guard.Against.NegativeOrZero(branchId, nameof(branchId));
		//	Description = description?.Trim();
		//	Sku = Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
		//	Barcode = Guard.Against.NullOrWhiteSpace(barcode, nameof(barcode));
		//	Status = Guard.Against.EnumOutOfRange(status, nameof(status));
		//	Image = image;
		//}
	}
}
