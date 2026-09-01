using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Tariffs;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Common;

internal static class OrderPricingResolver
{
    internal static async Task<Result<ResolvedOrderPricing>> ResolveAsync(
        IUnitOfWork unitOfWork,
        long branchId,
        long tariffId,
        IReadOnlyCollection<OrderItemSelectionModel> requestedItems,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
        TariffPricingSnapshot? tariff = await unitOfWork
            .Repository<Tariff>()
            .QueryAsync(x => x.Id == tariffId)
            .Select(x => new TariffPricingSnapshot
            {
                Id = x.Id,
                BranchId = x.BranchId,
                Disable = x.Disable,
                Status = x.Status,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (tariff is null)
        {
            return MissingTariff("Tariff not found.");
        }

        long[] serviceIds = requestedItems.Select(x => x.ServiceId).Distinct().ToArray();
        long[] unitRelationIds = requestedItems
            .Select(x => x.UnitRelationId)
            .Distinct()
            .ToArray();

        List<ServiceTariffPricingSnapshot> pricingRows = await unitOfWork
            .Repository<ServiceTariff>()
            .QueryAsync(x =>
                x.TariffId == tariffId
                && serviceIds.Contains(x.ServiceId)
                && unitRelationIds.Contains(x.UnitRelationId)
            )
            .Select(x => new ServiceTariffPricingSnapshot
            {
                TariffId = x.TariffId,
                ServiceId = x.ServiceId,
                UnitRelationId = x.UnitRelationId,
                Price = x.Price,
                ServiceName = x.Service.Name,
                ServiceBranchId = x.Service.BranchId,
                ServiceDisable = x.Service.Disable,
                ServiceStatus = x.Service.Status,
                UnitRelationServiceId = x.UnitRelation.ServiceId,
                UnitRelationName = x.UnitRelation.Name,
                UnitRelationPrice = x.UnitRelation.Price,
                ProcessingTime = x.UnitRelation.ProcessingTime,
                UnitRelationStatus = x.UnitRelation.Status,
            })
            .ToListAsync(cancellationToken);

        return Resolve(branchId, requestedItems, tariff, pricingRows, now);
    }

    internal static Result<ResolvedOrderPricing> Resolve(
        long branchId,
        IReadOnlyCollection<OrderItemSelectionModel> requestedItems,
        TariffPricingSnapshot tariff,
        IReadOnlyCollection<ServiceTariffPricingSnapshot> pricingRows,
        DateTimeOffset now
    )
    {
        if (branchId <= 0 || requestedItems.Count == 0)
        {
            return Invalid("Order must contain a valid branch and at least one item.");
        }

        if (requestedItems.Any(x => x.ServiceId <= 0 || x.UnitRelationId <= 0 || x.Quantity <= 0))
        {
            return Invalid("Order item selection and quantity must be positive.");
        }

        if (
            requestedItems.Select(x => (x.ServiceId, x.UnitRelationId)).Distinct().Count()
            != requestedItems.Count
        )
        {
            return Invalid("Duplicate service and unit relation combinations are not allowed.");
        }

        if (tariff.BranchId != branchId)
        {
            return Invalid("Tariff does not belong to the selected branch.");
        }

        if (
            tariff.Disable
            || tariff.Status != ActivationStatus.Active
            || (tariff.StartAt.HasValue && tariff.StartAt.Value > now)
            || (tariff.EndAt.HasValue && tariff.EndAt.Value < now)
        )
        {
            return Invalid("Tariff is not active at the current time.");
        }

        Dictionary<(long ServiceId, long UnitRelationId), ServiceTariffPricingSnapshot> rows =
            pricingRows.ToDictionary(x => (x.ServiceId, x.UnitRelationId));
        var resolvedItems = new List<ResolvedOrderItem>(requestedItems.Count);

        foreach (OrderItemSelectionModel requestedItem in requestedItems)
        {
            if (
                !rows.TryGetValue(
                    (requestedItem.ServiceId, requestedItem.UnitRelationId),
                    out ServiceTariffPricingSnapshot? row
                )
                || row.TariffId != tariff.Id
                || row.UnitRelationServiceId != requestedItem.ServiceId
            )
            {
                return MissingPricing(
                    $"No pricing exists for service {requestedItem.ServiceId} and unit relation {requestedItem.UnitRelationId} in tariff {tariff.Id}."
                );
            }

            if (row.ServiceBranchId != branchId)
            {
                return Invalid("Service does not belong to the selected branch.");
            }

            if (
                row.ServiceDisable
                || row.ServiceStatus != ActivationStatus.Active
                || row.UnitRelationStatus != ActivationStatus.Active
            )
            {
                return Invalid("Service or unit relation is not active.");
            }

            if (row.Price <= 0 || row.UnitRelationPrice < 0)
            {
                return Invalid("Authoritative service pricing is invalid.");
            }

            resolvedItems.Add(
                new ResolvedOrderItem
                {
                    ServiceId = requestedItem.ServiceId,
                    UnitRelationId = requestedItem.UnitRelationId,
                    Quantity = requestedItem.Quantity,
                    Price = row.Price,
                    UnitPrice = row.UnitRelationPrice,
                    ServiceName = row.ServiceName,
                    UnitRelationName = row.UnitRelationName,
                    ProcessingTime = row.ProcessingTime,
                }
            );
        }

        return Result<ResolvedOrderPricing>.Success(
            new ResolvedOrderPricing { Items = resolvedItems }
        );
    }

    private static Result<ResolvedOrderPricing> Invalid(string title) =>
        Result<ResolvedOrderPricing>.Failure(
            new BadRequestError(
                title,
                Messager.Create<Order>().Message(MessageType.Valid).Negative().Build()
            )
        );

    private static Result<ResolvedOrderPricing> MissingTariff(string title) =>
        Result<ResolvedOrderPricing>.Failure(
            new NotFoundError(
                title,
                Messager.Create<Tariff>().Message(MessageType.Existence).Negative().Build()
            )
        );

    private static Result<ResolvedOrderPricing> MissingPricing(string title) =>
        Result<ResolvedOrderPricing>.Failure(
            new NotFoundError(
                title,
                Messager.Create<ServiceTariff>().Message(MessageType.Existence).Negative().Build()
            )
        );
}

internal static class OrderPriceCalculator
{
    internal static Result<OrderPriceSummary> Calculate(
        IReadOnlyCollection<ResolvedOrderItem> items,
        bool discountFixed,
        decimal discountValue,
        int vatPercent
    )
    {
        if (
            items.Count == 0
            || items.Any(x => x.Price <= 0 || x.UnitPrice < 0 || x.Quantity <= 0)
            || discountValue < 0
            || vatPercent < 0
        )
        {
            return InvalidTotals("Order pricing contains a negative or invalid value.");
        }

        if (!discountFixed && discountValue > 100)
        {
            return InvalidTotals("Percentage discount must be between 0 and 100.");
        }

        decimal amount;
        decimal discountAmount;
        decimal subtotal;
        decimal vatAmount;
        decimal total;
        try
        {
            amount = items.Sum(x => x.Price * x.Quantity);
            discountAmount = discountFixed
                ? discountValue
                : amount * discountValue / 100;
            subtotal = amount - discountAmount;
            vatAmount = subtotal * vatPercent / 100;
            total = subtotal + vatAmount;
        }
        catch (OverflowException)
        {
            return InvalidTotals("Order totals are outside the supported range.");
        }

        if (discountAmount > amount)
        {
            return InvalidTotals("Discount cannot exceed the order amount.");
        }

        if (amount < 0 || subtotal < 0 || vatAmount < 0 || total < 0)
        {
            return InvalidTotals("Order totals cannot be negative.");
        }

        return Result<OrderPriceSummary>.Success(
            new OrderPriceSummary
            {
                Amount = amount,
                DiscountAmount = discountAmount,
                Subtotal = subtotal,
                VatAmount = vatAmount,
                Total = total,
            }
        );
    }

    private static Result<OrderPriceSummary> InvalidTotals(string title) =>
        Result<OrderPriceSummary>.Failure(
            new BadRequestError(
                title,
                Messager.Create<Order>().Message(MessageType.Valid).Negative().Build()
            )
        );
}

internal sealed record TariffPricingSnapshot
{
    public long Id { get; init; }
    public long BranchId { get; init; }
    public bool Disable { get; init; }
    public ActivationStatus Status { get; init; }
    public DateTimeOffset? StartAt { get; init; }
    public DateTimeOffset? EndAt { get; init; }
}

internal sealed record ServiceTariffPricingSnapshot
{
    public long TariffId { get; init; }
    public long ServiceId { get; init; }
    public long UnitRelationId { get; init; }
    public decimal Price { get; init; }
    public string ServiceName { get; init; } = default!;
    public long ServiceBranchId { get; init; }
    public bool ServiceDisable { get; init; }
    public ActivationStatus ServiceStatus { get; init; }
    public long? UnitRelationServiceId { get; init; }
    public string UnitRelationName { get; init; } = default!;
    public decimal UnitRelationPrice { get; init; }
    public decimal ProcessingTime { get; init; }
    public ActivationStatus UnitRelationStatus { get; init; }
}

internal sealed class ResolvedOrderPricing
{
    public IReadOnlyList<ResolvedOrderItem> Items { get; init; } = [];
}

internal sealed class ResolvedOrderItem
{
    public long ServiceId { get; init; }
    public long UnitRelationId { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal UnitPrice { get; init; }
    public string ServiceName { get; init; } = default!;
    public string UnitRelationName { get; init; } = default!;
    public decimal ProcessingTime { get; init; }
}

internal sealed class OrderPriceSummary
{
    public decimal Amount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal VatAmount { get; init; }
    public decimal Total { get; init; }
}

internal sealed class VoucherRedemption
{
    public long VoucherId { get; init; }
    public string Code { get; init; } = default!;
    public bool DiscountFixed { get; init; }
    public decimal DiscountValue { get; init; }
}
