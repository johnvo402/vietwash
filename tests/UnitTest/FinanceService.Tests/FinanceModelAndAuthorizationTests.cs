using System.Reflection;
using Application.Common.Auth;
using Application.Features.Funds.Events;
using Domain.Aggregates.Funds;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Presentation.Endpoints.Transactions;

namespace FinanceService.Tests;

public class FinanceModelAndAuthorizationTests
{
    [Fact]
    public void FundModel_HasFilteredUniqueSourceEventIndex()
    {
        var options = new DbContextOptionsBuilder<TheDbContext>()
            .UseNpgsql("Host=localhost;Database=model_validation")
            .Options;
        using var context = new TheDbContext(options);

        var fund = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(Fund))
        );
        var index = Assert.Single(
            fund.GetIndexes(),
            candidate => candidate.Properties.Single().Name == nameof(Fund.SourceEventId)
        );

        Assert.True(index.IsUnique);
        Assert.Equal(UpdateStatusOrderEventHandler.SourceEventIndexName, index.GetDatabaseName());
        Assert.Equal("source_event_id IS NOT NULL", index.GetFilter());
        Assert.True(index.Properties.Single().IsNullable);
    }

    [Fact]
    public void ArbitraryCustomerPointEndpoint_RequiresStaffRoles()
    {
        var authorization = GetAuthorizationAttribute(
            typeof(GetPointByCustomerEndpoint),
            typeof(Application.Features.Transactions.Queries.GetPointCustomer.GetPointCustomerQuery)
        );

        Assert.Contains("ADMIN", authorization.Value);
        Assert.Contains("MANAGER", authorization.Value);
        Assert.Contains("STAFF", authorization.Value);
        Assert.DoesNotContain("CUSTOMER", authorization.Value);
    }

    [Fact]
    public void SelfPointEndpoint_RemainsAvailableToAuthenticatedCustomer()
    {
        var authorization = GetAuthorizationAttribute(
            typeof(GetPointCustomerEndpoint),
            typeof(CancellationToken)
        );

        Assert.Equal(string.Empty, authorization.Value);
    }

    private static AuthorizeByAttribute GetAuthorizationAttribute(
        Type endpointType,
        Type firstParameterType
    )
    {
        var method = endpointType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(candidate =>
                candidate.Name == "HandleAsync"
                && candidate.GetParameters().First().ParameterType == firstParameterType
            );

        return Assert.IsType<AuthorizeByAttribute>(
            method.GetCustomAttribute<AuthorizeByAttribute>()
        );
    }
}
