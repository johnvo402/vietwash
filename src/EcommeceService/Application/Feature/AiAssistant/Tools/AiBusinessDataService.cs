using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.AiAssistant.Interfaces;
using Application.Feature.AiAssistant.Models;
using Application.Feature.Reports.Common;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.TopService;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Products;
using Domain.Functions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.AiAssistant.Tools;

public sealed class AiBusinessDataService(IUnitOfWork unitOfWork, ISender sender)
    : IAiBusinessDataService
{
    public async Task<RevenueSummaryResult> GetRevenueSummaryAsync(
        GetRevenueSummaryInput input,
        CancellationToken cancellationToken
    )
    {
        Result<IEnumerable<GetRevenueStatistic>> response = await sender.Send(
            new GetRevenueStatisticQuery
            {
                BranchId = input.BranchId.ToString(),
                From = input.From.ToString("yyyy-MM-dd"),
                To = input.To.ToString("yyyy-MM-dd"),
            },
            cancellationToken
        );
        IReadOnlyList<DailyRevenue> daily = RequireValue(response)
            .Select(item => new DailyRevenue(item.RevenueDate, item.TotalRevenue))
            .ToList();

        return new RevenueSummaryResult(
            input.BranchId,
            input.From,
            input.To,
            daily.Sum(item => item.Revenue),
            daily
        );
    }

    public async Task<OrderStatisticsResult> GetOrderStatisticsAsync(
        GetOrderStatisticsInput input,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    )
    {
        ReportUtcRange range = ReportTimeRange.ForLocalDates(input.From, input.To, timeZone);
        var counts = await unitOfWork
            .Repository<Order>()
            .QueryAsync(order =>
                order.BranchId == input.BranchId
                && order.CreatedAt >= range.UtcStartInclusive
                && order.CreatedAt < range.UtcEndExclusive
            )
            .AsNoTracking()
            .GroupBy(order => order.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        IReadOnlyList<OrderStatusCount> byStatus = counts
            .OrderBy(item => item.Status)
            .Select(item => new OrderStatusCount(item.Status.ToString(), item.Count))
            .ToList();
        return new OrderStatisticsResult(
            input.BranchId,
            input.From,
            input.To,
            byStatus.Sum(item => item.Count),
            byStatus
        );
    }

    public async Task<TopServicesResult> GetTopServicesAsync(
        GetTopServicesInput input,
        CancellationToken cancellationToken
    )
    {
        Result<IEnumerable<GetTopServiceResponse>> response = await sender.Send(
            new GetTopServiceQuery
            {
                BranchId = input.BranchId.ToString(),
                From = input.From.ToString("yyyy-MM-dd"),
                To = input.To.ToString("yyyy-MM-dd"),
            },
            cancellationToken
        );
        IReadOnlyList<TopServiceItem> services = RequireValue(response)
            .Take(input.Limit)
            .Select(item => new TopServiceItem(
                item.ServiceName ?? "Không xác định",
                item.UsageCount ?? 0,
                item.TotalRevenue ?? 0m
            ))
            .ToList();
        return new TopServicesResult(input.BranchId, input.From, input.To, services);
    }

    public async Task<InventoryAlertsResult> GetInventoryAlertsAsync(
        GetInventoryAlertsInput input,
        CancellationToken cancellationToken
    )
    {
        List<InventoryAlertItem> items = await unitOfWork
            .Repository<BranchProduct>()
            .QueryAsync(product => product.BranchId == input.BranchId && !product.Disable)
            .AsNoTracking()
            .Select(product => new
            {
                product.Name,
                product.Sku,
                CurrentStock = product
                    .ProductSupplyings.Where(supplying =>
                        supplying.InventoryDocument.Status == InventoryStatus.Completed
                    )
                    .Sum(supplying => supplying.Quantity * supplying.UnitRelation.Multiple),
            })
            .Where(item => item.CurrentStock <= input.LowStockThreshold)
            .OrderBy(item => item.CurrentStock)
            .ThenBy(item => item.Name)
            .Take(100)
            .Select(item => new InventoryAlertItem(item.Name, item.Sku, item.CurrentStock))
            .ToListAsync(cancellationToken);

        return new InventoryAlertsResult(input.BranchId, input.LowStockThreshold, items);
    }

    public async Task<EquipmentUtilizationResult> GetEquipmentUtilizationAsync(
        GetEquipmentUtilizationInput input,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    )
    {
        ReportUtcRange range = ReportTimeRange.ForLocalDates(input.From, input.To, timeZone);
        var equipment = await unitOfWork
            .Repository<Equipment>()
            .QueryAsync(item => item.BranchId == input.BranchId)
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Active = group.Count(item => item.Status == EquipmentStatus.Active),
                InUse = group.Count(item => item.Using),
            })
            .SingleOrDefaultAsync(cancellationToken);

        var assignments = await unitOfWork
            .Repository<OrderEquipment>()
            .QueryAsync(item =>
                item.Equipment.BranchId == input.BranchId
                && item.Order.Status != OrderStatus.Cancelled
                && item.Order.CreatedAt >= range.UtcStartInclusive
                && item.Order.CreatedAt < range.UtcEndExclusive
            )
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Count = group.Count(),
                DistinctEquipment = group.Select(item => item.EquipmentId).Distinct().Count(),
            })
            .SingleOrDefaultAsync(cancellationToken);

        int active = equipment?.Active ?? 0;
        int distinctUsed = assignments?.DistinctEquipment ?? 0;
        int total = equipment?.Total ?? 0;
        decimal utilization = total == 0
            ? 0m
            : Math.Round((decimal)distinctUsed / total * 100m, 2);
        return new EquipmentUtilizationResult(
            input.BranchId,
            input.From,
            input.To,
            total,
            active,
            equipment?.InUse ?? 0,
            distinctUsed,
            assignments?.Count ?? 0,
            utilization
        );
    }

    private static T RequireValue<T>(Result<T> result)
        where T : class
    {
        if (result.IsSuccess)
            return result.Value!;
        if (result.Error?.Status == StatusCodes.Status403Forbidden)
            throw new AiToolForbiddenException("The requested branch is not authorized.");
        throw new AiToolExecutionException("Business data could not be retrieved.");
    }
}
