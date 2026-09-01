using System.Linq.Expressions;
using Domain.Aggregates.Orders;

namespace Application.Feature.Reports.Common;

/// <summary>
/// Defines the financial meaning shared by dashboard and revenue reports.
/// Point redemption is disabled, so point discount is explicitly zero.
/// </summary>
public static class OrderRevenuePolicy
{
    public static IQueryable<OrderRevenueRow> SelectCompletedRevenueRows(
        this IQueryable<Order> orders,
        ReportUtcRange range,
        IReadOnlyCollection<long> branchIds
    ) =>
        orders
            .Where(order =>
                order.Status == Domain.Aggregates.Orders.Enums.OrderStatus.Completed
                && order.OrderDate.HasValue
                && order.OrderDate >= range.UtcStartInclusive
                && order.OrderDate < range.UtcEndExclusive
                && branchIds.Contains(order.BranchId)
            )
            .Select(Projection);

    public static OrderRevenueMetrics Calculate(
        decimal amount,
        bool discountFixed,
        decimal discountValue,
        decimal vatAmount,
        decimal collectedAmount
    )
    {
        decimal grossAmount = Math.Max(0m, amount);
        decimal discountAmount = CalculateDiscountAmount(
            grossAmount,
            discountFixed,
            discountValue
        );

        return new OrderRevenueMetrics(
            GrossAmount: grossAmount,
            DiscountAmount: discountAmount,
            NetBeforeVat: Math.Max(0m, grossAmount - discountAmount),
            VatAmount: vatAmount,
            CollectedAmount: collectedAmount
        );
    }

    public static OrderRevenueMetrics Calculate(Order order) =>
        Calculate(
            order.Amount,
            order.DiscountFixed,
            order.DiscountValue,
            order.VatAmount,
            order.Total
        );

    public static decimal CalculateDiscountAmount(
        decimal grossAmount,
        bool discountFixed,
        decimal discountValue
    )
    {
        grossAmount = Math.Max(0m, grossAmount);
        discountValue = Math.Max(0m, discountValue);

        decimal discountAmount = discountFixed
            ? discountValue
            : grossAmount * discountValue / 100m;

        return Math.Min(grossAmount, discountAmount);
    }

    public static decimal AllocateDiscountToLine(
        decimal lineGross,
        decimal orderGross,
        decimal orderDiscountAmount
    ) =>
        orderGross > 0m
            ? Math.Min(Math.Max(0m, lineGross), orderGross)
                * Math.Min(Math.Max(0m, orderDiscountAmount), orderGross)
                / orderGross
            : 0m;

    public static decimal CalculatePercentageChange(decimal current, decimal previous)
    {
        if (previous == 0m)
            return current == 0m ? 0m : 100m;

        return ((current - previous) / previous) * 100m;
    }

    public static int CountRegisteredCustomers(IEnumerable<long?> customerIds) =>
        customerIds.Where(customerId => customerId.HasValue).Distinct().Count();

    public static int CalculateServiceUsage(IEnumerable<OrderItem> items) =>
        items.Sum(item => item.Quantity);

    /// <summary>
    /// EF-translatable projection. Only Completed orders with a non-null OrderDate
    /// should be supplied by callers.
    /// </summary>
    public static Expression<Func<Order, OrderRevenueRow>> Projection =>
        order => new OrderRevenueRow
        {
            OrderId = order.Id,
            BranchId = order.BranchId,
            CustomerId = order.CustomerId,
            FinancialDate = order.OrderDate!.Value,
            GrossAmount = order.Amount <= 0m ? 0m : order.Amount,
            DiscountAmount =
                order.Amount <= 0m || order.DiscountValue <= 0m
                    ? 0m
                    : order.DiscountFixed
                        ? (order.DiscountValue > order.Amount
                            ? order.Amount
                            : order.DiscountValue)
                        : order.DiscountValue >= 100m
                            ? order.Amount
                            : order.Amount * order.DiscountValue / 100m,
            VatAmount = order.VatAmount,
            CollectedAmount = order.Total,
        };

    public static IQueryable<ServiceRevenueLineRow> SelectServiceRevenueLines(
        this IQueryable<Order> orders
    ) =>
        orders
            .SelectMany(order =>
                order.OrderItems.Select(item => new ServiceRevenueSourceRow
                {
                    OrderId = order.Id,
                    ServiceId = item.ServiceId,
                    ServiceName = item.ServiceName ?? string.Empty,
                    UnitId = item.UnitRelationId,
                    UnitName = item.UnitRelationName ?? string.Empty,
                    LineGross = item.Price * item.Quantity,
                    OrderGross = order.Amount <= 0m ? 0m : order.Amount,
                    OrderDiscountAmount =
                        order.Amount <= 0m || order.DiscountValue <= 0m
                            ? 0m
                            : order.DiscountFixed
                                ? (order.DiscountValue > order.Amount
                                    ? order.Amount
                                    : order.DiscountValue)
                                : order.DiscountValue >= 100m
                                    ? order.Amount
                                    : order.Amount * order.DiscountValue / 100m,
                })
            )
            .Select(line => new ServiceRevenueLineRow
            {
                OrderId = line.OrderId,
                ServiceId = line.ServiceId,
                ServiceName = line.ServiceName,
                UnitId = line.UnitId,
                UnitName = line.UnitName,
                GrossAmount = line.LineGross,
                DiscountAmount =
                    line.OrderGross > 0m
                        ? line.OrderDiscountAmount * line.LineGross / line.OrderGross
                        : 0m,
            });
}

public readonly record struct OrderRevenueMetrics(
    decimal GrossAmount,
    decimal DiscountAmount,
    decimal NetBeforeVat,
    decimal VatAmount,
    decimal CollectedAmount
);

public sealed class OrderRevenueRow
{
    public long OrderId { get; init; }
    public long BranchId { get; init; }
    public long? CustomerId { get; init; }
    public DateTimeOffset FinancialDate { get; init; }
    public decimal GrossAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal VatAmount { get; init; }
    public decimal CollectedAmount { get; init; }
}

public sealed class ServiceRevenueLineRow
{
    public long OrderId { get; init; }
    public long ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public long UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal GrossAmount { get; init; }
    public decimal DiscountAmount { get; init; }
}

internal sealed class ServiceRevenueSourceRow
{
    public long OrderId { get; init; }
    public long ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public long UnitId { get; init; }
    public string UnitName { get; init; } = string.Empty;
    public decimal LineGross { get; init; }
    public decimal OrderGross { get; init; }
    public decimal OrderDiscountAmount { get; init; }
}
