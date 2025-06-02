using Domain.Aggregates.Accounts.Enums;

namespace Application.Features.Common.Projections.Accounts;

public class AccountDetailProjection : AccountProjection
{
    public AccountStatus Status { get; set; }
    public Gender Gender { get; set; }
    public DateOnly BirthDay { get; set; }
    public List<AccountContactProjection>? AccountContacts { get; set; }
    public List<BranchAccountProjection>? BranchAccounts { get; set; }
}
