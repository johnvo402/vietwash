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

namespace Application.Feature.Reports.Queries.ServiceOrderReport;

public class ServiceRevenueReportHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<
        ServiceRevenueReportQuery,
        Result<PaginationResponse<ServiceRevenueReportResponse>>
    >
{
    public async ValueTask<Result<PaginationResponse<ServiceRevenueReportResponse>>> Handle(
        ServiceRevenueReportQuery request,
        CancellationToken cancellationToken
    )
    {
        ReportBranchScopeResult branchScope = ReportBranchScope.Resolve(
            currentUser.Session?.Branches,
            request.BranchIds
        );
        if (branchScope.HasUnauthorizedBranch)
            return Result<PaginationResponse<ServiceRevenueReportResponse>>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        ReportUtcRange range = ReportTimeRange.ForUnixSeconds(request.From, request.To);
        IQueryable<Order> completedOrders = unitOfWork
            .Repository<Order>()
            .QueryAsync(order =>
                order.Status == OrderStatus.Completed
                && order.OrderDate.HasValue
                && order.OrderDate >= range.UtcStartInclusive
                && order.OrderDate < range.UtcEndExclusive
                && branchScope.BranchIds.Contains(order.BranchId)
            );

        PaginationResponse<ServiceRevenueReportResponse> query = await completedOrders
            .SelectServiceRevenueLines()
            .GroupBy(line => new
            {
                line.ServiceId,
                line.ServiceName,
                line.UnitId,
                line.UnitName,
            })
            .Select(group => new ServiceRevenueReportResponse
            {
                ServiceId = group.Key.ServiceId,
                ServiceName = group.Key.ServiceName,
                UnitId = group.Key.UnitId,
                UnitName = group.Key.UnitName,
                TotalOrderCount = group.Select(line => line.OrderId).Distinct().Count(),
                TotalRevenue = group.Sum(line => line.GrossAmount),
                TotalDiscount = group.Sum(line => line.DiscountAmount),
                TotalNetRevenue = group.Sum(line => line.GrossAmount - line.DiscountAmount),
            })
            .Search(request.Keyword, request.Targets)
            .Sort($"TotalOrderCount{OrderTerm.DELIMITER}{OrderTerm.DESC}")
            .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

        return Result<PaginationResponse<ServiceRevenueReportResponse>>.Success(query);
    }
}
