using Domain.Aggregates.Accounts;

namespace Application.Features.Common.Projections.Accounts
{
    public class BranchAccountProjection
    {
        public long AccountId { get; set; }
        public long BranchId { get; set; }
        public string? BranchName { get; set; }

        public virtual void MappingFrom(BranchAccount branchAccount)
        {
            AccountId = branchAccount.AccountId;
            BranchId = branchAccount.BranchId;
            BranchName = branchAccount.BranchName;
        }
    }
}
