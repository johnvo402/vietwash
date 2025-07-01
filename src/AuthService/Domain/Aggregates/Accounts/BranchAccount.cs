using Shared.Kernel.Common;

namespace Domain.Aggregates.Accounts
{
    public class BranchAccount : BaseEntity
    {
        public long AccountId { get; set; }
        public long BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}
