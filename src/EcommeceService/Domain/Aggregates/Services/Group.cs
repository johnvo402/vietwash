using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Services
{
    public class Group : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; } = default!;
        public bool Disable { get; set; } = default!;

        public IEnumerable<GroupService> GroupServices { get; set; } = [];

    }
}
