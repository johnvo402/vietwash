using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.PubSubLogs;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.DistributedCache;

public class PubSubFactory(IServiceScopeFactory serviceScopeFactory) : IPubSubFactory
{
    public IPubSubService GetPubSub(PubSubType type)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        return type switch
        {
            PubSubType.Origin => provider.GetRequiredService<PubSubService>(),
            PubSubType.DeadLetter => provider.GetRequiredService<DeadLetterPubSubService>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid PubSub provider"),
        };
    }
}
