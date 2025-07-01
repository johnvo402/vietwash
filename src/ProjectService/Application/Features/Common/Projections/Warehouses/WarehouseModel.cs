

using Domain.Aggregates.Enums;

namespace Application.Features.Common.Projections.Warehouses
{
    public class WarehouseModel
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;
    }
}
