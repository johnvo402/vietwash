using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Common;

// Shared read-only selection resolution. Only CreateOrder may claim the returned voucher.
internal static class OrderPricingReader
{
    internal static async Task<Result<OrderPricingSelection>> ReadAsync(
        IUnitOfWork unitOfWork,
        long branchId,
        long tariffId,
        long customerId,
        string? voucherCode,
        IReadOnlyCollection<OrderItemSelectionModel> items,
        int vatPercent,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
        var pricing = await OrderPricingResolver.ResolveAsync(
            unitOfWork,
            branchId,
            tariffId,
            items,
            now,
            cancellationToken
        );
        if (pricing.IsFailure)
            return Result<OrderPricingSelection>.Failure(pricing.Error!);

        VoucherRedemption? voucher = null;
        if (!string.IsNullOrWhiteSpace(voucherCode))
        {
            voucher = await unitOfWork
                .Repository<Voucher>()
                .QueryAsync(VoucherEligibility.ForCustomer(voucherCode.Trim(), customerId, now))
                .Select(x => new VoucherRedemption
                {
                    VoucherId = x.Id,
                    Code = x.Code,
                    DiscountFixed = x.DiscountFixed,
                    DiscountValue = x.DiscountValue,
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (voucher is null)
                return Result<OrderPricingSelection>.Failure(
                    new NotFoundError(
                        "Voucher is invalid, inactive, expired, used, or not assigned to this customer.",
                        Messager.Create<Voucher>().Message(MessageType.Valid).Negative().Build()
                    )
                );
        }

        var totals = OrderPriceCalculator.Calculate(
            pricing.Value!.Items,
            voucher?.DiscountFixed ?? false,
            voucher?.DiscountValue ?? 0,
            vatPercent
        );
        return totals.IsFailure
            ? Result<OrderPricingSelection>.Failure(totals.Error!)
            : Result<OrderPricingSelection>.Success(new(pricing.Value!, totals.Value!, voucher));
    }
}

internal sealed record OrderPricingSelection(
    ResolvedOrderPricing Pricing,
    OrderPriceSummary Totals,
    VoucherRedemption? Voucher
);
