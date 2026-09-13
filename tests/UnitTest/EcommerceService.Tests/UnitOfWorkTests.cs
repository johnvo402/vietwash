using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Infrastructure.UnitOfWorks;
using Infrastructure.UnitOfWorks.CachedRepositories;
using Infrastructure.UnitOfWorks.Repositories;
using Moq;
using Serilog;

namespace EcommerceService.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public void DynamicReadOnlyRepository_WhenCachingIsDisabled_ReturnsNormalRepository()
    {
        using var unitOfWork = CreateUnitOfWork();

        IDynamicSpecificationRepository<TestEntity> repository =
            unitOfWork.DynamicReadOnlyRepository<TestEntity>(isCached: false);

        Assert.IsType<DynamicSpecificationRepository<TestEntity>>(repository);
    }

    [Fact]
    public void DynamicReadOnlyRepository_WhenCachingIsEnabled_ReturnsCachedRepository()
    {
        using var unitOfWork = CreateUnitOfWork();

        IDynamicSpecificationRepository<TestEntity> repository =
            unitOfWork.DynamicReadOnlyRepository<TestEntity>(isCached: true);

        Assert.IsType<CachedDynamicSpecRepository<TestEntity>>(repository);
    }

    [Fact]
    public void DynamicReadOnlyRepository_WhenCalledAgainWithSameTypeAndConfiguration_ReturnsSameCachedInstance()
    {
        using var unitOfWork = CreateUnitOfWork();

        IDynamicSpecificationRepository<TestEntity> first =
            unitOfWork.DynamicReadOnlyRepository<TestEntity>(isCached: true);
        IDynamicSpecificationRepository<TestEntity> second =
            unitOfWork.DynamicReadOnlyRepository<TestEntity>(isCached: true);

        Assert.Same(first, second);
    }

    private static UnitOfWork CreateUnitOfWork() =>
        new(
            Mock.Of<IDbContext>(),
            Mock.Of<ILogger>(),
            Mock.Of<IMemoryCacheService>()
        );

    private sealed class TestEntity;
}
