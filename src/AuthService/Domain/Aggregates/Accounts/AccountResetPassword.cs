using Shared.Kernel.Common;

namespace Domain.Aggregates.Accounts;

public class AccountResetPassword : BaseEntity
{
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset Expiry { get; set; }

    public long AccountId { get; set; }

    public Account? Account { get; set; }
}
