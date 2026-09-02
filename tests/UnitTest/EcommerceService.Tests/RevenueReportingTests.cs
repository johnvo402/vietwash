using Application.Feature.Reports.Common;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace EcommerceService.Tests;

public class RevenueReportingTests
{
    private static readonly DateTimeOffset RangeStart = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly ReportUtcRange Range = new(RangeStart, RangeStart.AddDays(1));

    [Theory]
    [InlineData(100, true, 10, 10)]
    [InlineData(100, false, 10, 10)]
    [InlineData(100, false, 25, 25)]
    [InlineData(100, true, 150, 100)]
    [InlineData(100, false, 150, 100)]
    [InlineData(100, true, -10, 0)]
    [InlineData(-100, false, 10, 0)]
    public void DiscountAmount_IsMonetaryAndSafelyBounded(
        decimal gross,
        bool fixedDiscount,
        decimal value,
        decimal expected
    ) =>
        Assert.Equal(
            expected,
            OrderRevenuePolicy.CalculateDiscountAmount(gross, fixedDiscount, value)
        );

    [Fact]
    public void Metrics_DefineGrossDiscountNetVatAndCollectedSeparately()
    {
        OrderRevenueMetrics metrics = OrderRevenuePolicy.Calculate(
            amount: 100m,
            discountFixed: false,
            discountValue: 10m,
            vatAmount: 9m,
            collectedAmount: 99m
        );

        Assert.Equal(100m, metrics.GrossAmount);
        Assert.Equal(10m, metrics.DiscountAmount);
        Assert.Equal(90m, metrics.NetBeforeVat);
        Assert.Equal(9m, metrics.VatAmount);
        Assert.Equal(99m, metrics.CollectedAmount);
    }

    [Fact]
    public void CollectedAmount_RemainsAuthoritative()
    {
        OrderRevenueMetrics metrics = OrderRevenuePolicy.Calculate(100m, false, 10m, 9m, 123m);

        Assert.Equal(123m, metrics.CollectedAmount);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(100, 0, 100)]
    [InlineData(0, 100, -100)]
    [InlineData(150, 100, 50)]
    [InlineData(50, 100, -50)]
    public void PercentageChange_UsesDefinedZeroRules(
        decimal current,
        decimal previous,
        decimal expected
    ) => Assert.Equal(expected, OrderRevenuePolicy.CalculatePercentageChange(current, previous));

    [Fact]
    public void BranchScope_RejectsUnauthorizedSingleBranch()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["1", "2"], [3]);

        Assert.True(result.HasUnauthorizedBranch);
    }

    [Fact]
    public void BranchScope_RejectsMixedAuthorizedAndUnauthorizedBranches()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["1", "2"], [1, 3]);

        Assert.True(result.HasUnauthorizedBranch);
    }

    [Fact]
    public void BranchScope_UsesAllAuthorizedBranchesWhenRequestIsMissing()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["2", "1"], null);

        Assert.False(result.HasUnauthorizedBranch);
        Assert.Equal([1L, 2L], result.BranchIds);
    }

    [Fact]
    public void BranchScope_DoesNotTreatInvalidSessionBranchAsAuthorized()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["invalid", "1"], [2]);

        Assert.True(result.HasUnauthorizedBranch);
    }

    [Fact]
    public void BranchScope_DeduplicatesRequestedBranches()
    {
        ReportBranchScopeResult result = ReportBranchScope.Resolve(["1"], [1, 1]);

        Assert.False(result.HasUnauthorizedBranch);
        Assert.Single(result.BranchIds);
    }

    [Fact]
    public void LocalDay_ConvertsVietnamMidnightToUtc()
    {
        TimeZoneInfo timeZone = FixedTimeZone(TimeSpan.FromHours(7));

        ReportUtcRange range = ReportTimeRange.ForLocalDay(new DateOnly(2026, 9, 2), timeZone);

        Assert.Equal(
            new DateTimeOffset(2026, 9, 1, 17, 0, 0, TimeSpan.Zero),
            range.UtcStartInclusive
        );
        Assert.Equal(
            new DateTimeOffset(2026, 9, 2, 17, 0, 0, TimeSpan.Zero),
            range.UtcEndExclusive
        );
    }

    [Fact]
    public void LocalDateRange_IsHalfOpenAtNextLocalMidnight()
    {
        TimeZoneInfo timeZone = FixedTimeZone(TimeSpan.FromHours(7));

        ReportUtcRange range = ReportTimeRange.ForLocalDates(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 2),
            timeZone
        );

        Assert.Equal(TimeSpan.FromDays(2), range.UtcEndExclusive - range.UtcStartInclusive);
    }

    [Fact]
    public void UnixRange_TreatsToSecondAsInclusiveAndProducesExclusiveBoundary()
    {
        ReportUtcRange range = ReportTimeRange.ForUnixSeconds(100, 200);

        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(100), range.UtcStartInclusive);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(201), range.UtcEndExclusive);
    }

    [Fact]
    public void InvalidTimeZone_FallsBackToUtc() =>
        Assert.Equal(TimeZoneInfo.Utc, ReportTimeRange.ResolveTimeZone("not/a-real-zone"));

    [Fact]
    public void LocalDateParser_PreservesRequestedCalendarDate()
    {
        DateOnly date = ReportTimeRange.ParseLocalDate("2026-09-02T23:59:59.000Z", "from");

        Assert.Equal(new DateOnly(2026, 9, 2), date);
    }

    [Fact]
    public void LocalDateEnumerator_EmitsMissingChartDays()
    {
        DateOnly[] dates = ReportTimeRange
            .EnumerateLocalDates(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3))
            .ToArray();

        Assert.Equal(
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2), new DateOnly(2026, 9, 3)],
            dates
        );
    }

    [Fact]
    public void SameCalendarDayLastMonth_IsNotAnEntireMonth() =>
        Assert.Equal(new DateOnly(2026, 8, 2), new DateOnly(2026, 9, 2).AddMonths(-1));

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.InProgress)]
    [InlineData(OrderStatus.Processed)]
    [InlineData(OrderStatus.Cancelled)]
    public void NonCompletedStatuses_NeverCountAsEarnedRevenue(OrderStatus status)
    {
        Order order = CreateOrder(status, branchId: 1, gross: 100m, total: 100m, RangeStart);

        Assert.Empty(SelectRevenue([order], [1]));
    }

    [Fact]
    public void CompletedOrder_CountsAsEarnedRevenue()
    {
        Order order = CreateOrder(
            OrderStatus.Completed,
            branchId: 1,
            gross: 100m,
            total: 99m,
            RangeStart
        );

        Assert.Equal(99m, Assert.Single(SelectRevenue([order], [1])).CollectedAmount);
    }

    [Fact]
    public void AuthorizedBranchA_NeverIncludesBranchB()
    {
        Order branchA = CreateOrder(OrderStatus.Completed, 1, 100m, 90m, RangeStart);
        Order branchB = CreateOrder(OrderStatus.Completed, 2, 200m, 180m, RangeStart);

        OrderRevenueRow row = Assert.Single(SelectRevenue([branchA, branchB], [1]));

        Assert.Equal(1, row.BranchId);
        Assert.Equal(90m, row.CollectedAmount);
    }

    [Fact]
    public void RevenueRange_UsesOrderDateRatherThanCreationDate()
    {
        Order completedOutsideRange = CreateOrder(
            OrderStatus.Completed,
            1,
            100m,
            100m,
            Range.UtcEndExclusive.AddMinutes(1)
        );

        Assert.Empty(SelectRevenue([completedOutsideRange], [1]));
    }

    [Fact]
    public void FinancialRange_IncludesExactStart()
    {
        Order order = CreateOrder(OrderStatus.Completed, 1, 100m, 100m, Range.UtcStartInclusive);

        Assert.Single(SelectRevenue([order], [1]));
    }

    [Fact]
    public void FinancialRange_ExcludesExactEnd()
    {
        Order order = CreateOrder(OrderStatus.Completed, 1, 100m, 100m, Range.UtcEndExclusive);

        Assert.Empty(SelectRevenue([order], [1]));
    }

    [Fact]
    public void ServiceDiscount_IsAllocatedProportionallyAndReconciles()
    {
        Order order = CreateOrder(
            OrderStatus.Completed,
            1,
            gross: 1000m,
            total: 900m,
            RangeStart,
            discountFixed: false,
            discountValue: 10m
        );
        order.OrderItems =
        [
            CreateItem(serviceId: 1, price: 600m, quantity: 1),
            CreateItem(serviceId: 2, price: 400m, quantity: 1),
        ];

        ServiceRevenueLineRow[] lines = new[] { order }
            .AsQueryable()
            .SelectServiceRevenueLines()
            .ToArray();

        Assert.Equal(60m, lines.Single(line => line.ServiceId == 1).DiscountAmount);
        Assert.Equal(40m, lines.Single(line => line.ServiceId == 2).DiscountAmount);
        Assert.Equal(100m, lines.Sum(line => line.DiscountAmount));
        Assert.Equal(900m, lines.Sum(line => line.GrossAmount - line.DiscountAmount));
    }

    [Fact]
    public void ServiceUsage_CountsQuantityRatherThanRows()
    {
        OrderItem[] items =
        [
            CreateItem(serviceId: 1, price: 10m, quantity: 5),
            CreateItem(serviceId: 1, price: 10m, quantity: 2),
        ];

        Assert.Equal(7, OrderRevenuePolicy.CalculateServiceUsage(items));
    }

    [Fact]
    public void UniqueCustomerCount_ExcludesGuests()
    {
        int count = OrderRevenuePolicy.CountRegisteredCustomers([null, null, 10, 10, 11]);

        Assert.Equal(2, count);
    }

    [Fact]
    public void CancellationValue_RemainsSeparateWithoutInflatingRevenue()
    {
        Order completed = CreateOrder(OrderStatus.Completed, 1, 100m, 90m, RangeStart);
        Order cancelled = CreateOrder(OrderStatus.Cancelled, 1, 500m, 500m, RangeStart);

        decimal revenue = SelectRevenue([completed, cancelled], [1])
            .Sum(row => row.CollectedAmount);
        decimal cancellationValue = new[] { completed, cancelled }
            .Where(order => order.Status == OrderStatus.Cancelled)
            .Sum(order => order.Amount);

        Assert.Equal(90m, revenue);
        Assert.Equal(500m, cancellationValue);
    }

    [Fact]
    public void CustomerRevenueSql_AggregatesCompletedAndCancelledStatusesSeparately()
    {
        string sql = CustomerRevenueSql();

        Assert.Contains(
            $"CASE WHEN o.status = {(byte)OrderStatus.Completed} THEN o.amount ELSE 0 END",
            sql
        );
        Assert.Contains(
            $"CASE WHEN o.status = {(byte)OrderStatus.Cancelled} THEN o.amount ELSE 0 END",
            sql
        );
        Assert.Contains(
            $"CASE WHEN o.status = {(byte)OrderStatus.Completed} THEN o.total ELSE 0 END",
            sql
        );
    }

    [Fact]
    public void CustomerRevenueSql_UsesOrderDateForRevenueAndHalfOpenRanges()
    {
        string sql = CustomerRevenueSql();

        Assert.Contains("o.order_date >= _from", sql);
        Assert.Contains("o.order_date < _to", sql);
        Assert.DoesNotContain("o.order_date <= _to", sql);
    }

    [Fact]
    public void CustomerRevenueSql_UsesCancelledAtForCancellationMetrics()
    {
        string sql = CustomerRevenueSql();

        Assert.Contains("o.cancelled_at IS NOT NULL", sql);
        Assert.Contains("o.cancelled_at >= _from", sql);
        Assert.Contains("o.cancelled_at < _to", sql);
        Assert.DoesNotContain("o.delivery_time >= _from", sql);
    }

    [Fact]
    public void SharedCompletedOrders_ReconcileAcrossRevenueViews()
    {
        Order[] fixture =
        [
            CreateOrder(OrderStatus.Pending, 1, 100m, 100m, RangeStart),
            CreateOrder(OrderStatus.InProgress, 1, 200m, 200m, RangeStart),
            CreateOrder(OrderStatus.Processed, 1, 300m, 300m, RangeStart),
            CreateOrder(OrderStatus.Completed, 1, 1000m, 990m, RangeStart),
            CreateOrder(OrderStatus.Cancelled, 1, 500m, 500m, RangeStart),
            CreateOrder(OrderStatus.Completed, 2, 2000m, 1980m, RangeStart),
        ];

        OrderRevenueRow[] rows = SelectRevenue(fixture, [1]);
        decimal dashboard = rows.Sum(row => row.CollectedAmount);
        decimal revenueStatistic = rows.GroupBy(row => row.FinancialDate.Date)
            .Sum(group => group.Sum(row => row.CollectedAmount));
        decimal branchRevenue = rows.GroupBy(row => row.BranchId)
            .Sum(group => group.Sum(row => row.CollectedAmount));
        decimal revenueReport = rows.Sum(row => row.CollectedAmount);
        decimal financialReport = rows.Sum(row => row.CollectedAmount);

        Assert.Equal(990m, dashboard);
        Assert.Equal(dashboard, revenueStatistic);
        Assert.Equal(dashboard, branchRevenue);
        Assert.Equal(dashboard, revenueReport);
        Assert.Equal(dashboard, financialReport);
    }

    private static OrderRevenueRow[] SelectRevenue(
        IEnumerable<Order> orders,
        IReadOnlyCollection<long> branchIds
    ) => orders.AsQueryable().SelectCompletedRevenueRows(Range, branchIds).ToArray();

    private static Order CreateOrder(
        OrderStatus status,
        long branchId,
        decimal gross,
        decimal total,
        DateTimeOffset orderDate,
        bool discountFixed = false,
        decimal discountValue = 0m
    ) =>
        new(
            branchId: branchId,
            staffId: 1,
            code: Guid.NewGuid().ToString("N"),
            amount: gross,
            total: total,
            status: status,
            discountFixed: discountFixed,
            discountValue: discountValue
        )
        {
            OrderDate = orderDate,
        };

    private static OrderItem CreateItem(long serviceId, decimal price, int quantity) =>
        new()
        {
            ServiceId = serviceId,
            ServiceName = $"Service {serviceId}",
            UnitRelationId = 1,
            UnitRelationName = "Unit",
            Price = price,
            Quantity = quantity,
        };

    private static TimeZoneInfo FixedTimeZone(TimeSpan offset) =>
        TimeZoneInfo.CreateCustomTimeZone($"UTC{offset}", offset, "Fixed", "Fixed");

    private static string CustomerRevenueSql() =>
        File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "Migrations",
                "get_customer_revenue_report_v2",
                "up.sql"
            )
        );
}
