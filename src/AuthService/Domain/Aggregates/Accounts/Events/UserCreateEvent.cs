using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Accounts.Events;

public class AccountCreateEvent : IDomainEvent
{
    public Account Account { get; set; } = default!;
}
