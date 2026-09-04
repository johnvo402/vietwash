using Infrastructure.Services.DistributedCache;
using Moq;
using Shared.Kernel.Extensions;
using StackExchange.Redis;

namespace EcommerceService.Tests;

public class RedisCacheServiceTests
{
    [Fact]
    public void Service_ReusesDatabaseFromInjectedMultiplexer()
    {
        var database = new Mock<IDatabase>();
        var multiplexer = new Mock<IConnectionMultiplexer>();
        multiplexer.Setup(redis => redis.GetDatabase(-1, null)).Returns(database.Object);

        var service = new RedisCacheService(multiplexer.Object);

        Assert.Same(database.Object, service.Database);
        Assert.Same(database.Object, service.Database);
        multiplexer.Verify(redis => redis.GetDatabase(-1, null), Times.Once);
    }

    [Fact]
    public void GetOrSet_ReturnsCachedValueWithoutInvokingFactory()
    {
        const string key = "cache-key";
        string serialized = SerializerExtension.Serialize("cached-value").StringJson;
        var database = new Mock<IDatabase>();
        database
            .Setup(redis => redis.StringGet((RedisKey)key, CommandFlags.None))
            .Returns((RedisValue)serialized);
        var service = CreateService(database);
        bool factoryInvoked = false;

        string? result = service.GetOrSet(
            key,
            () =>
            {
                factoryInvoked = true;
                return "source-value";
            }
        );

        Assert.Equal("cached-value", result);
        Assert.False(factoryInvoked);
    }

    [Fact]
    public void GetOrSet_CachesFactoryValueOnMiss()
    {
        const string key = "cache-key";
        TimeSpan expiry = TimeSpan.FromMinutes(5);
        var database = new Mock<IDatabase>();
        database
            .Setup(redis => redis.StringGet((RedisKey)key, CommandFlags.None))
            .Returns(RedisValue.Null);
        var service = CreateService(database);

        string? result = service.GetOrSet(key, () => "source-value", expiry);

        Assert.Equal("source-value", result);
        IInvocation setInvocation = Assert.Single(
            database.Invocations,
            invocation => invocation.Method.Name == nameof(IDatabase.StringSet)
        );
        Assert.Equal(expiry, setInvocation.Arguments[2]);
    }

    private static RedisCacheService CreateService(Mock<IDatabase> database)
    {
        var multiplexer = new Mock<IConnectionMultiplexer>();
        multiplexer.Setup(redis => redis.GetDatabase(-1, null)).Returns(database.Object);
        return new RedisCacheService(multiplexer.Object);
    }
}
