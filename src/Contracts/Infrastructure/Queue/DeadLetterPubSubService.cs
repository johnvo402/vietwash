using Application.Common.Interfaces.Services.DistributedCache;
using JohnChum.SharedKernel.Extensions;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;

namespace Infrastructure.Services.DistributedCache;

public class DeadLetterPubSubService(
    IConnectionMultiplexer redis,
    IOptions<PubSubSettings> options,
    ILogger logger
) : IPubSubService
{
    private readonly ISubscriber subscriber = redis.GetSubscriber();
    private readonly PubSubSettings settings = options.Value;

    public async Task<bool> PublishAsync<T>(T payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        try
        {
            var message = SerializerExtension.Serialize(payload).StringJson;
            var channel = GetChannelName(typeof(T));

            var result = await subscriber.PublishAsync(RedisChannel.Literal(channel), message);

            if (result == 0)
            {
                logger.Warning(
                    "[PubSubService] Published to channel {Channel} but no subscribers found",
                    channel
                );
            }
            else
            {
                logger.Debug(
                    "[PubSubService] Published to channel {Channel} with {SubscriberCount} subscribers",
                    channel,
                    result
                );
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "[PubSubService] Failed to publish message of type {Type}",
                typeof(T).Name
            );
            return false;
        }
    }

    public void Subscribe<T>(Func<T, Task> handler)
    {
        var channel = GetChannelName(typeof(T));

        subscriber.Subscribe(
            RedisChannel.Literal(channel),
            async (_, message) =>
            {
                try
                {
                    var result = SerializerExtension.Deserialize<T>(message!);
                    if (result.Object is T payload)
                    {
                        await handler(payload);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(
                        ex,
                        "[DeadLetterPubSubService] Failed to handle message on channel {Channel}",
                        channel
                    );
                }
            }
        );
    }

    public Task<bool> PingAsync()
    {
        try
        {
            var result = redis.GetDatabase().Ping();
            return Task.FromResult(result.TotalMilliseconds > 0);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "[DeadLetterPubSubService] Redis ping failed");
            return Task.FromResult(false);
        }
    }

    private string GetChannelName(Type type) => $"{settings.ChannelPrefix}:deadletter:{type.Name}";
}
