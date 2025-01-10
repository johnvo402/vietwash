using System.Text.Json;
using MediatR;
using Micro.Shared.Domain;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Micro.Shared.Infrastructure.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publishEndpoint;
    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;
    private readonly ICurrentUser _currentUser;

    public DispatchDomainEventsInterceptor(IPublisher publishEndpoint, ILogger<DispatchDomainEventsInterceptor> logger, ICurrentUser currentUser)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _currentUser = currentUser;
    }
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null) return;

        var domainEvents = context.ChangeTracker.Entries<Entity>()
       .SelectMany(entry => entry.Entity.PopDomainEvents())
       .ToList();
        if (!domainEvents.Any()) return;

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation($"OccurredOn: {domainEvent.OccurredOn} - Event: {domainEvent.GetType().Name} - Data: {JsonSerializer.Serialize(domainEvent.Data)} - UserAccess: {JsonSerializer.Serialize(_currentUser)}");
            await _publishEndpoint.Publish(domainEvent);
        }
    }
}
