using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Functions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Statistics.Queries.RevenueStatistic;

public class GetRevenueStatisticHandler(
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetRevenueStatisticQuery, Result<IEnumerable<GetRevenueStatistic>>>
{
    public async ValueTask<Result<IEnumerable<GetRevenueStatistic>>> Handle(
        GetRevenueStatisticQuery request,
        CancellationToken cancellationToken
    )
    {
        // 1. Lấy timezone từ header request (mặc định UTC nếu không có)
        var tzId = httpContextAccessor.HttpContext?.Request.Headers["Time-Zone"].ToString();
        TimeZoneInfo tz;
        try
        {
            tz = !string.IsNullOrEmpty(tzId)
                ? TimeZoneInfo.FindSystemTimeZoneById(tzId)
                : TimeZoneInfo.Utc;
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.Utc;
        }

        // 2. Parse input date (theo timezone)
        var startDate = DateTime.Parse(request.From);
        var endDate = DateTime.Parse(request.To);

        var branchId = long.TryParse(request.BranchId, out var bId) ? bId : (long?)null;

        // 3. Query doanh thu -> convert sang timezone trước khi lấy Date
        var orders = await unitOfWork
            .Repository<Order>()
            .QueryAsync(o =>
                o.Status == OrderStatus.Completed
                && (!branchId.HasValue || o.BranchId == branchId.Value)
                && o.OrderDate >= startDate
                && o.OrderDate <= endDate
            )
            .ToListAsync(cancellationToken); // <- async load dữ liệu

        var revenueByDate = orders
            .GroupBy(o =>
            {
                if (!o.OrderDate.HasValue)
                    return DateTime.MinValue;
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(o.OrderDate.Value.UtcDateTime, tz);
                return localTime.Date;
            })
            .ToDictionary(g => DateOnly.FromDateTime(g.Key), g => g.Sum(o => o.Amount));

        // 4. Generate danh sách ngày đầy đủ (theo timezone)
        var totalDays = (endDate.Date - startDate.Date).Days + 1;
        var allDates = Enumerable
            .Range(0, totalDays)
            .Select(offset => DateOnly.FromDateTime(startDate.AddDays(offset)));

        // 5. Merge kết quả
        var result = allDates
            .Select(date => new GetRevenueStatistic
            {
                RevenueDate = date,
                TotalRevenue = revenueByDate.TryGetValue(date, out var total) ? total : 0,
            })
            .OrderBy(x => x.RevenueDate)
            .ToList();

        return Result<IEnumerable<GetRevenueStatistic>>.Success(result);
    }
}
