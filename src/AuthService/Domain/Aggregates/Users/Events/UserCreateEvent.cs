using Mediator;

namespace Domain.Aggregates.Users.Events;

public class UserCreateEvent : INotification
{
    public User User { get; set; } = default!;
}
