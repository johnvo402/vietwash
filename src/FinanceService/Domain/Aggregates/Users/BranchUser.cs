using Shared.Kernel.Common;

namespace Domain.Aggregates.Users
{
    public class BranchUser : BaseEntity<long>
    {
        public long UserId { get; set; }
        public long BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}
