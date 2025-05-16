using Domain.Aggregates.Accounts.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Accounts
{
    public class AccountActivity : BaseEntity<long>
    {
        public long AccountId { get; set; }
        public string? Ip { get; set; }
        public AccountActivityType Type { get; set; }
        public string? Position { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public virtual Account Account { get; set; } = default!;
    }
}
