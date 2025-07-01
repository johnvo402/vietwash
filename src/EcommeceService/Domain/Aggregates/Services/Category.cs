using Ardalis.GuardClauses;
using Domain.Aggregates.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
    public class Category : BaseEntity<long>
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string? ParentId { get; set; }
        public ActivationStatus Status { get; set; } = default!;
        public bool Disabled { get; set; }
        public ICollection<Service> Services { get; set; } = [];

        public Category(string name, string? parentId, ActivationStatus status, string code)
        {
            Name = Guard.Against.Null(name, nameof(Name));
            ParentId = parentId;
            Status = status;
            Code = Guard.Against.NullOrEmpty(code, nameof(Code));
            Path = string.IsNullOrWhiteSpace(parentId) ? name : $"{parentId}/{name}";
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
