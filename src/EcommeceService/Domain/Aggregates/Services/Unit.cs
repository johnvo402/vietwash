using Domain.Aggregates.Services.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
    public class Unit : BaseEntity<long>
    {
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;

        public void Update(string? name = null, ActivationStatus? status = null)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }

            if (status.HasValue)
            {
                Status = status.Value;
            }
        }

        public Unit(string name, ActivationStatus status)
        {
            Name = name;
            Status = status;
        }
    }
}
