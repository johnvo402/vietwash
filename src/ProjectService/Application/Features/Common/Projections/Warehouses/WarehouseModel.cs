using Domain.Aggregates.Warehouses.Enums;

namespace Application.Features.Common.Projections.Warehouses
{
    public class WarehouseModel
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public WarehouseStatus Status { get; set; } = default!;
    }
}
