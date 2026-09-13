using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Application.Common.Events;
using Shared.Kernel.Common;
using Shared.Kernel.Common.Events;

namespace Infrastructure.Data.Interceptors;

public class DispatchDomainEventInterceptor(IServiceScopeFactory serviceScopeFactory)
    : SaveChangesInterceptor
{
    // Intentionally not dispatched from SavedChanges: an explicit outer transaction may
    // still roll back. UnitOfWork invokes this only after commit (or an auto-committed save).
    public async Task DispatchDomainEventsAsync(
        DbContext? context,
        CancellationToken cancellationToken = default
    )
    {
        if (context == null)
            return;

        IEnumerable<AggregateRoot> entities = context
            .ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.UncommittedEvents.Count != 0)
            .Select(e => e.Entity);

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IPublisher mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
        foreach (AggregateRoot entity in entities.ToList())
        {
            while (entity.UncommittedEvents.FirstOrDefault() is IDomainEvent domainEvent)
            {
                await mediator.Publish(
                    (object)new DomainEventNotification(domainEvent),
                    cancellationToken
                );
                if (!entity.TryDequeueUncommittedEvent(out _))
                    throw new InvalidOperationException("The dispatched domain event was not queued.");
            }
        }
    }
}
