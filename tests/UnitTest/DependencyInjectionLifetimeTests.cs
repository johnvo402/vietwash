using Application;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using Mediator;

// Linked into each service's isolated test assembly (their namespaces intentionally overlap).
public class DependencyInjectionLifetimeTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    public async Task RealRegistrationsValidateScopes_AndShareOneContextPerRequest(string environment)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseSettings:DatabaseConnection"] = "Host=127.0.0.1;Database=vietwash_seed_test;Username=test;Password=test",
            ["RedisDatabaseSettings:Host"] = "127.0.0.1",
            ["RedisDatabaseSettings:Port"] = "6379",
            ["RedisDatabaseSettings:IsEnbaled"] = "true",
            ["SecuritySettings:JwtSettings:SecretKey"] = "bootstrap-test-key-not-for-runtime-1234567890",
            ["SecuritySettings:JwtSettings:Issuer"] = "test",
            ["SecuritySettings:JwtSettings:Audience"] = "test",
            ["SecuritySettings:EncryptionOptions:Key"] = "0123456789abcdef0123456789abcdef",
            ["SecuritySettings:EncryptionOptions:IV"] = "0123456789abcdef",
            ["S3AwsSettings:ServiceUrl"] = "http://127.0.0.1:9000",
            ["S3AwsSettings:PublicUrl"] = "/image",
            ["S3AwsSettings:PreSignedUrlExpirationInMinutes"] = "60",
            ["S3AwsSettings:AccessKey"] = "test",
            ["S3AwsSettings:SecretKey"] = "test",
            ["S3AwsSettings:BucketName"] = "test",
            ["OtpOption:ApiKey"] = "test",
            ["OtpOption:DomainUrl"] = "https://sms.example.test",
            ["HangfireSettings:Enable"] = "true",
            ["HangfireSettings:Storage:ConnectionString"] = "Host=127.0.0.1;Database=vietwash_seed_test;Username=test;Password=test",
            ["PayOsSetting:IsEnabled"] = "false",
        });
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<ILogger>(Log.Logger);
        builder.Services.AddInfrastructureDependencies(builder.Configuration, environment);
        builder.Services.AddApplicationDependencies();
        await using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true, ValidateScopes = true,
        });
        await using var first = provider.CreateAsyncScope();
        await using var second = provider.CreateAsyncScope();
        var firstDb = first.ServiceProvider.GetRequiredService<TheDbContext>();
        Assert.Same(firstDb, first.ServiceProvider.GetRequiredService<IDbContext>());
        Assert.NotSame(firstDb, second.ServiceProvider.GetRequiredService<TheDbContext>());
        var audit = first.ServiceProvider.GetRequiredService<UpdateAuditableEntityInterceptor>();
        Assert.NotSame(audit, second.ServiceProvider.GetRequiredService<UpdateAuditableEntityInterceptor>());
        Assert.NotSame(first.ServiceProvider.GetRequiredService<ICurrentAccount>(), second.ServiceProvider.GetRequiredService<ICurrentAccount>());
        Assert.Contains(audit, firstDb.GetService<IDbContextOptions>().Extensions.OfType<CoreOptionsExtension>().Single().Interceptors!);
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<UpdateAuditableEntityInterceptor>());
        // ValidateOnBuild skips open generics. Resolve a closed pipeline as well:
        // LoggingBehavior depends on the request-scoped current account.
        Assert.NotEmpty(first.ServiceProvider.GetServices<IPipelineBehavior<IMessage, object>>());
        Assert.Throws<InvalidOperationException>(() => provider.GetServices<IPipelineBehavior<IMessage, object>>().ToArray());
    }
}
