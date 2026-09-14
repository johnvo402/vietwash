using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.UpdateStatus;

internal static class OrderCancellationCoordinator
{
    public static async Task<ErrorDetails?> SchedulePayOsAsync(
        IUnitOfWork unitOfWork,
        Order order,
        OrderCancellation cancellation,
        CancellationToken cancellationToken
    )
    {
        var requests = unitOfWork.Repository<PayOsCancellationRequest>();
        PayOsCancellationRequest? existing = await requests.FindByConditionAsync(
            request => request.OrderId == order.Id,
            cancellationToken
        );
        if (existing?.IsTerminalFailure == true)
            return OrderStatusRequestPolicy.BadRequest(
                existing.LastError ?? "The payment link could not be cancelled safely."
            );

        if (existing is null)
            _ = await requests.AddAsync(
                PayOsCancellationRequest.Create(order.Id, cancellation),
                cancellationToken
            );

        await unitOfWork.SaveAsync(cancellationToken);
        return null;
    }

    public static async Task<ErrorDetails?> ReleaseLocalResourcesAsync(
        IUnitOfWork unitOfWork,
        Order order,
        OrderCancellationResourcePlan plan,
        CancellationToken cancellationToken
    )
    {
        if (!plan.IsCancellation)
            return null;

        bool hasVoucherUsage = await unitOfWork
            .Repository<VoucherUsage>()
            .AnyAsync(usage => usage.OrderId == order.Id, cancellationToken);
        if (hasVoucherUsage)
            return OrderStatusRequestPolicy.BadRequest(
                "Order cancellation invariant violated: completed voucher usage already exists."
            );

        if (!plan.ShouldReleaseVoucher)
            return null;

        long voucherId =
            order.VoucherId
            ?? throw new InvalidOperationException(
                "Voucher release was planned without a voucher id."
            );
        long customerId =
            order.CustomerId
            ?? throw new InvalidOperationException(
                "Voucher release was planned without a customer id."
            );
        _ = await unitOfWork
            .Repository<VoucherCustomer>()
            .QueryAsync(customer =>
                customer.VoucherId == voucherId
                && customer.CustomerId == customerId
                && customer.IsUsed
            )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(customer => customer.IsUsed, false),
                cancellationToken
            );
        return null;
    }
}
