
namespace Application.Feature.Common.Projections.EquipmentActivities
{
	public class EquipmentActivityDetailModel
	{
		public string PartName { get; set; } = default!;
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
	}
}
