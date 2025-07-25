using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Equipments.Enums;

namespace Application.Feature.Common.Projections.EquipmentActivities
{
    public class EquipmentActivityModel
    {
        public TypeActivity Type { get; set; }
        public string? Description { get; set; }
        public decimal LaborCost { get; set; }
        public List<EquipmentActivityDetailModel> Details { get; set; } = [];
	}
}
