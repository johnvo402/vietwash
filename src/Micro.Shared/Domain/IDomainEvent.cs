using MediatR;

namespace Micro.Shared.Domain;

public interface IDomainEvent : INotification
{
    public DateTimeOffset OccurredOn { get; }
    public object? Data { get; set;}
}