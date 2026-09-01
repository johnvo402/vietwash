using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;

namespace Application.Feature.Reports.RevenueReport;

public class RevenueReportHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<RevenueReportQuery, Result<PaginationResponse<RevenueReportResponse>>>
{
    public async ValueTask<Result<PaginationResponse<RevenueReportResponse>>> Handle(
        RevenueReportQuery request,
        CancellationToken cancellationToken
    )
    {
        ReportBranchScopeResult branchScope = ReportBranchScope.Resolve(
            currentUser.Session?.Branches,
            request.BranchIds
        );
        if (branchScope.HasUnauthorizedBranch)
            return Result<PaginationResponse<RevenueReportResponse>>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        ReportUtcRange range = ReportTimeRange.ForUnixSeconds(request.From, request.To);
        IQueryable<OrderRevenueRow> revenueRows = unitOfWork
            .Repository<Order>()
            .QueryAsync()
            .SelectCompletedRevenueRows(range, branchScope.BranchIds);

        PaginationResponse<RevenueReportResponse> query = await revenueRows
            // Unix report filters are absolute instants, so the response date is the UTC financial date.
            .GroupBy(row => new { Date = row.FinancialDate.Date, row.BranchId })
            .Select(group => new RevenueReportResponse
            {
                Date = DateOnly.FromDateTime(group.Key.Date),
                BranchId = group.Key.BranchId,
                OrderQuantity = group.Count(),
                // Guests are not treated as one registered customer.
                CustomerQuantity = group
                    .Where(row => row.CustomerId.HasValue)
                    .Select(row => row.CustomerId)
                    .Distinct()
                    .Count(),
                TotalRevenue = group.Sum(row => row.GrossAmount),
                TotalDiscount = group.Sum(row => row.DiscountAmount),
                TotalNetRevenue = group.Sum(row => row.CollectedAmount),
                AverageRevenuePerOrder = Math.Round(
                    group.Sum(row => row.CollectedAmount) / group.Count(),
                    2
                ),
                AverageRevenuePerCustomer = group
                        .Where(row => row.CustomerId.HasValue)
                        .Select(row => row.CustomerId)
                        .Distinct()
                        .Count() == 0
                    ? 0m
                    : Math.Round(
                        group.Sum(row => row.CollectedAmount)
                            / group
                                .Where(row => row.CustomerId.HasValue)
                                .Select(row => row.CustomerId)
                                .Distinct()
                                .Count(),
                        2
                    ),
            })
            .Search(request.Keyword, request.Targets)
            .Sort($"TotalRevenue{OrderTerm.DELIMITER}{OrderTerm.DESC}")
            .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

        return Result<PaginationResponse<RevenueReportResponse>>.Success(query);
    }
}
