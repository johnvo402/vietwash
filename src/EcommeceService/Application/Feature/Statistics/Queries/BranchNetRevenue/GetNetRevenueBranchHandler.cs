using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Contracts.ApiWrapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Statistics.Queries.BranchNetRevenue;

public class GetNetRevenueBranchHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentUser,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetNetRevenueBranchQuery, Result<IEnumerable<GetNetRevenueBranchResponse>>>
{
    public async ValueTask<Result<IEnumerable<GetNetRevenueBranchResponse>>> Handle(
        GetNetRevenueBranchQuery request,
        CancellationToken cancellationToken
    )
    {
        ReportBranchScopeResult branchScope = ReportBranchScope.Resolve(
            currentUser.Session?.Branches,
            null
        );
        TimeZoneInfo timeZone = ReportTimeRange.ResolveTimeZone(
            httpContextAccessor.HttpContext?.Request.Headers["Time-Zone"].ToString()
        );
        DateOnly fromDate = ReportTimeRange.ParseLocalDate(request.From, nameof(request.From));
        DateOnly toDate = ReportTimeRange.ParseLocalDate(request.To, nameof(request.To));
        ReportUtcRange range = ReportTimeRange.ForLocalDates(fromDate, toDate, timeZone);

        List<GetNetRevenueBranchResponse> revenueByBranch = await unitOfWork
            .Repository<Order>()
            .QueryAsync()
            .SelectCompletedRevenueRows(range, branchScope.BranchIds)
            .GroupBy(row => row.BranchId)
            .Select(group => new GetNetRevenueBranchResponse
            {
                BranchId = group.Key,
                TotalNetRevenue = group.Sum(row => row.CollectedAmount),
            })
            .ToListAsync(cancellationToken);

        decimal totalRevenue = revenueByBranch.Sum(branch => branch.TotalNetRevenue);
        if (totalRevenue > 0m)
            foreach (GetNetRevenueBranchResponse branch in revenueByBranch)
                branch.Percentage = (float)
                    Math.Round(branch.TotalNetRevenue / totalRevenue * 100m, 2);

        return Result<IEnumerable<GetNetRevenueBranchResponse>>.Success(revenueByBranch);
    }
}
