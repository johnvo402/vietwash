using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Common.Messages;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderHandler(
        IUnitOfWork unitOfWork,
        ICurrentAccount currentAccount,
        OrgSetting orgSetting,
        IEncryptionService encryption,
        IQrGenerator barcode
    ) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
    {
        public async ValueTask<Result<CreateOrderResponse>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken
        )
        {
            bool customerExists = await unitOfWork
                .Repository<User>()
                .AnyAsync(
                    x => x.Id == request.CustomerId && x.Status == ActivationStatus.Active,
                    cancellationToken
                );
            if (!customerExists)
            {
                return Result<CreateOrderResponse>.Failure(
                    new NotFoundError(
                        "Active customer not found.",
                        Messager.Create<User>().Message(MessageType.Existence).Negative().Build()
                    )
                );
            }

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                DateTimeOffset now = DateTimeOffset.UtcNow;

                Result<ResolvedOrderPricing> pricing = await OrderPricingResolver.ResolveAsync(
                    unitOfWork,
                    request.BranchId,
                    request.TariffId,
                    request.OrderItems,
                    now,
                    cancellationToken
                );
                if (pricing.IsFailure)
                {
                    return await RollbackFailure(pricing.Error!, cancellationToken);
                }

                VoucherRedemption? voucher = null;
                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    string voucherCode = request.VoucherCode.Trim();
                    voucher = await unitOfWork
                        .Repository<Voucher>()
                        .QueryAsync(
                            VoucherEligibility.ForCustomer(voucherCode, request.CustomerId, now)
                        )
                        .Select(x => new VoucherRedemption
                        {
                            VoucherId = x.Id,
                            Code = x.Code,
                            DiscountFixed = x.DiscountFixed,
                            DiscountValue = x.DiscountValue,
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (voucher is null)
                    {
                        return await RollbackFailure(
                            new NotFoundError(
                                "Voucher is invalid, inactive, expired, used, or not assigned to this customer.",
                                Messager
                                    .Create<Voucher>()
                                    .Message(MessageType.Valid)
                                    .Negative()
                                    .Build()
                            ),
                            cancellationToken
                        );
                    }
                }

                Result<OrderPriceSummary> totals = OrderPriceCalculator.Calculate(
                    pricing.Value!.Items,
                    voucher?.DiscountFixed ?? false,
                    voucher?.DiscountValue ?? 0,
                    orgSetting.VatPercent
                );
                if (totals.IsFailure)
                {
                    return await RollbackFailure(totals.Error!, cancellationToken);
                }

                if (voucher is not null)
                {
                    int claimedRows = await unitOfWork
                        .Repository<VoucherCustomer>()
                        .QueryAsync(x =>
                            x.VoucherId == voucher.VoucherId
                            && x.CustomerId == request.CustomerId
                            && !x.IsUsed
                        )
                        .ExecuteUpdateAsync(
                            setters => setters.SetProperty(x => x.IsUsed, true),
                            cancellationToken
                        );
                    if (claimedRows < 1)
                    {
                        return await RollbackFailure(
                            new BadRequestError(
                                "Voucher was already used by another order.",
                                Messager
                                    .Create<VoucherCustomer>()
                                    .Message(MessageType.Valid)
                                    .Negative()
                                    .Build()
                            ),
                            cancellationToken
                        );
                    }
                }

                Order order = request.ToEntity(
                    (long)currentAccount.Id!,
                    orgSetting.VatPercent,
                    pricing.Value!,
                    totals.Value!,
                    voucher
                );
                order.CodeConfirm = barcode.GenerateQrBase64(encryption.Encrypt(order.Code));

                Order orderResult = await unitOfWork
                    .Repository<Order>()
                    .AddAsync(order, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                CreateOrderResponse? response = await unitOfWork
                    .DynamicReadOnlyRepository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(orderResult.Id),
                        x => x.ToCreateOrderResponse(),
                        cancellationToken
                    );

                return Result<CreateOrderResponse>.Success(response!);
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<Result<CreateOrderResponse>> RollbackFailure(
            ErrorDetails error,
            CancellationToken cancellationToken
        )
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return Result<CreateOrderResponse>.Failure(error);
        }
    }
}
