using Domain.Aggregates.Enums;
using Domain.Aggregates.Services.Enums;
using Mediator;
using Shared.Kernel.Common;

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

        public Category(string name, string? parentId, ActivationStatus status)
        {
            Name = name;
            ParentId = parentId;
            Status = status;
        }

        public void Update(string? name, string? parentId, ActivationStatus? status)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            ParentId = parentId;

            if (status.HasValue)
                Status = status.Value;
        }
    }
}
