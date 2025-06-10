using Domain.Aggregates.Services.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Services
{
    public class Category : BaseEntity<string>
    {
        public string Name { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string? ParentId { get; set; }
        public ActivationStatus Status { get; set; } = default!;
        public bool Disabled { get; set; }
        public ICollection<Service> Services { get; set; } = [];
    }
}
