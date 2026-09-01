using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Reports.FinancialReport;

public class FinancialReportHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<FinancialReportQuery, Result<FinancialReportResponse>>
{
    public async ValueTask<Result<FinancialReportResponse>> Handle(
        FinancialReportQuery request,
        CancellationToken cancellationToken
    )
    {
        ReportBranchScopeResult branchScope = ReportBranchScope.Resolve(
            currentUser.Session?.Branches,
            request.BranchIds
        );
        if (branchScope.HasUnauthorizedBranch)
            return Result<FinancialReportResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

        ReportUtcRange range = ReportTimeRange.ForUnixSeconds(request.From, request.To);
        var completed = await unitOfWork
            .Repository<Order>()
            .QueryAsync()
            .SelectCompletedRevenueRows(range, branchScope.BranchIds)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                GrossAmount = group.Sum(row => row.GrossAmount),
                DiscountAmount = group.Sum(row => row.DiscountAmount),
                CollectedAmount = group.Sum(row => row.CollectedAmount),
            })
            .SingleOrDefaultAsync(cancellationToken);

        // Cancellation has no financial OrderDate. Keep the existing operational CreatedAt
        // period for this separate informational metric; it never contributes to revenue.
        decimal cancelledValue = await unitOfWork
            .Repository<Order>()
            .QueryAsync(order =>
                order.Status == OrderStatus.Cancelled
                && order.CreatedAt >= range.UtcStartInclusive
                && order.CreatedAt < range.UtcEndExclusive
                && branchScope.BranchIds.Contains(order.BranchId)
            )
            .SumAsync(order => order.Amount, cancellationToken);

        return Result<FinancialReportResponse>.Success(
            new FinancialReportResponse
            {
                TotalRevenue = completed?.GrossAmount ?? 0m,
                CancelValue = cancelledValue,
                TotalDiscount = completed?.DiscountAmount ?? 0m,
                TotalPoint = 0m,
                TotalNetRevenue = completed?.CollectedAmount ?? 0m,
            }
        );
    }
}
