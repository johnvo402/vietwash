using Shared.Kernel.Common;

namespace Domain.Aggregates.Accounts;

public class AccountToken : BaseEntity<long>
{
    public string? Token { get; set; }
    public string? ClientIp { get; set; }
    public string? FamilyId { get; set; }
    public long AccountId { get; set; }
    public Account? Account { get; set; }
    public DateTimeOffset ExpiredTime { get; set; }
}
