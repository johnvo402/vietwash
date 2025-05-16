using Mediator;

namespace Domain.Aggregates.Accounts.Events;

public class AccountCreateEvent : INotification
{
    public Account Account { get; set; } = default!;
}
