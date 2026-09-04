using System.Reflection;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Report.Common;
using Application.Features.Report.FinanceReport;
using Contracts.Application.Common.Exceptions;
using Moq;
using Presentation.Endpoints.Reports;

namespace FinanceService.Tests;

public class ReportAuthorizationTests
{
    [Fact]
    public void FinancialReport_RequiresAdministratorOrManager()
    {
        var method = typeof(FinancialReportEndpoint)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Single(candidate => candidate.Name == "HandleAsync");
        var authorization = Assert.IsType<AuthorizeByAttribute>(
            method.GetCustomAttribute<AuthorizeByAttribute>()
        );

        Assert.Contains("\"ADMIN\"", authorization.Value);
        Assert.Contains("\"MANAGER\"", authorization.Value);
        Assert.DoesNotContain("\"STAFF\"", authorization.Value);
        Assert.DoesNotContain("\"CUSTOMER\"", authorization.Value);
    }

    [Fact]
    public void ReportBranchScope_UsesAllAuthorizedBranches_WhenNoFilterIsRequested()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["2", "1"], null);

        Assert.False(result.HasUnauthorizedBranch);
        Assert.Equal([1L, 2L], result.BranchIds);
    }

    [Fact]
    public void ReportBranchScope_RejectsAnyUnauthorizedRequestedBranch()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["1", "2"], [1, 3]);

        Assert.True(result.HasUnauthorizedBranch);
    }

    [Fact]
    public async Task FinancialReportHandler_RejectsUnauthorizedBranchBeforeQueryingData()
    {
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var currentAccount = new Mock<ICurrentAccount>();
        currentAccount.SetupGet(account => account.Session).Returns(
            new UserAuth { Id = 10, Role = "MANAGER", Branches = ["1"] }
        );
        var handler = new FinancialReportHandler(unitOfWork.Object, currentAccount.Object);

        var result = await handler.Handle(
            new FinancialReportQuery { From = 0, To = 1, BranchIds = [2] },
            default
        );

        Assert.IsType<ForbiddenError>(result.Error);
        unitOfWork.VerifyNoOtherCalls();
    }
}
