using Mediator;
using Shared.Kernel.Common.Events;

namespace Contracts.Application.Common.Events;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
