using Shared.Kernel.Common;

namespace Domain.Aggregates.Branches
{
    public class BranchUser : BaseEntity
    {
        public long UserId { get; set; }
        public long BranchId { get; set; }
        public bool Manager { get; set; }
        public virtual Branch Branch { get; set; } = default!;
    }
}
