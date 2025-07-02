using Domain.Aggregates.Accounts.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Accounts
{
    public class AccountActivity : BaseEntity
    {
        public long AccountId { get; set; }
        public string? Ip { get; set; }
        public AccountActivityType Type { get; set; }
        public string? Position { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public virtual Account Account { get; set; } = default!;
    }
}
