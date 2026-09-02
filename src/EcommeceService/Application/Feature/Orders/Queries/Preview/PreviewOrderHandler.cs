using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Feature.Orders.Queries.Preview;

public sealed class PreviewOrderHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    OrgSetting orgSetting
) : IRequestHandler<PreviewOrderQuery, Result<PreviewOrderResponse>>
{
    public async ValueTask<Result<PreviewOrderResponse>> Handle(
        PreviewOrderQuery request,
        CancellationToken cancellationToken
    )
    {
        if (
            !OrderActorAccess.CanOperateOrder(
                currentAccount.Session?.Role,
                currentAccount.Session?.Branches,
                request.BranchId
            )
        )
            return Result<PreviewOrderResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

        if (
            !await unitOfWork
                .Repository<User>()
                .AnyAsync(
                    x => x.Id == request.CustomerId && x.Status == ActivationStatus.Active,
                    cancellationToken
                )
        )
            return Result<PreviewOrderResponse>.Failure(
                new NotFoundError(
                    "Active customer not found.",
                    Messager.Create<User>().Message(MessageType.Existence).Negative().Build()
                )
            );

        var selection = await OrderPricingReader.ReadAsync(
            unitOfWork,
            request.BranchId,
            request.TariffId,
            request.CustomerId,
            request.VoucherCode,
            request.OrderItems ?? [],
            orgSetting.VatPercent,
            DateTimeOffset.UtcNow,
            cancellationToken
        );
        if (selection.IsFailure)
            return Result<PreviewOrderResponse>.Failure(selection.Error!);

        var (pricing, totals, voucher) = selection.Value!;
        return Result<PreviewOrderResponse>.Success(
            new PreviewOrderResponse
            {
                Amount = totals.Amount,
                DiscountAmount = totals.DiscountAmount,
                DiscountFixed = voucher?.DiscountFixed ?? false,
                DiscountValue = voucher?.DiscountValue ?? 0,
                NetBeforeVat = totals.Subtotal,
                VatPercent = orgSetting.VatPercent,
                VatAmount = totals.VatAmount,
                Total = totals.Total,
                OrderItems = pricing
                    .Items.Select(x => new PreviewOrderLine
                    {
                        ServiceId = x.ServiceId,
                        ServiceName = x.ServiceName,
                        UnitRelationId = x.UnitRelationId,
                        UnitRelationName = x.UnitRelationName,
                        UnitPrice = x.Price,
                        Quantity = x.Quantity,
                        LineAmount = x.Price * x.Quantity,
                    })
                    .ToArray(),
            }
        );
    }
}
