using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Common.Projections.Accounts;

public class AccountDetailProjection : AccountProjection
{
    public Gender Gender { get; set; }
    public DateOnly BirthDay { get; set; }
    public ICollection<AccountContactProjection>? AccountContacts { get; set; }
    public ICollection<BranchAccountProjection>? BranchAccounts { get; set; }

    public override void MappingFrom(Account account)
    {
        base.MappingFrom(account);
        AccountContacts = account
            .AccountContacts?.Select(accountContact =>
            {
                var userResonse = new AccountContactProjection();
                userResonse.MappingFrom(accountContact!);
                return userResonse;
            })
            .ToList();
        BranchAccounts = account
            .BranchAccounts?.Select(branchAccount =>
            {
                var userResonse = new BranchAccountProjection();
                userResonse.MappingFrom(branchAccount!);
                return userResonse;
            })
            .ToList();
    }
}
