using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Common.Projections.Accounts;

public class AccountDetailProjection : AccountProjection
{
    public Gender Gender { get; set; }
    public DateOnly BirthDay { get; set; }
    public AccountContactProjection? AccountContact { get; set; }
    public ICollection<BranchAccountProjection>? BranchAccounts { get; set; }

    public override void MappingFrom(Account account)
    {
        base.MappingFrom(account);
        BirthDay = account.BirthDay;
        Gender = account.Gender ?? Gender.Other;

        if (account.AccountContact != null)
        {
            AccountContact?.MappingFrom(account.AccountContact);
        }

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
