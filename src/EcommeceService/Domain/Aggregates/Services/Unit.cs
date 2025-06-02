using Domain.Aggregates.Equipments;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Services.Enums;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Services
{
    public class Unit : BaseEntity<long>
    {
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;

    }
}
