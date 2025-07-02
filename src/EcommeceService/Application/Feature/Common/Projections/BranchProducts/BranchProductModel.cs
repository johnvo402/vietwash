using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.BranchProducts
{
	public class BranchProductModel
	{
		public long BranchId { get; set; } = default!;
		public string Name { get; set; }
		public string Description { get; set; }
		public string Sku { get; set; }
		public string Barcode { get; set; }
		public string? Image { get; set; }
		public ActivationStatus Status { get; set; } = ActivationStatus.Active;
		public List<UnitRelationModel> UnitRelations { get; set; } = [];

	}
}
