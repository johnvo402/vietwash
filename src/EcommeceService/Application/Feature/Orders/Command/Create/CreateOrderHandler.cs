using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.Detail;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Mediator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderHandler(
        IUnitOfWork unitOfWork,
        ICurrentAccount _currentAccount,
        IEncryptionService encryption,
        IQrGenerator barcode
    ) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
    {
        public async ValueTask<Result<CreateOrderResponse>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken
        )
        {
            Order order = request.ToEntity((long)_currentAccount.Id!);

            var codeEncrypt = encryption.Encrypt(order.Code);
            var barcodeConfirm = barcode.GenerateQrBase64(codeEncrypt);
            order.CodeConfirm = barcodeConfirm;

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                var orderRes = await unitOfWork
                    .Repository<Order>()
                    .AddAsync(order, cancellationToken);

                VoucherCustomer? voucherCustomer = null;
                decimal discountApply = 0;
                if (order.CustomerId != null && order.VoucherCode != null)
                {
                    Voucher getVoucher = await unitOfWork
                        .DynamicReadOnlyRepository<Voucher>()
                        .FindByConditionAsync(
                            new GetVoucherByCodeSpecification(order.VoucherCode),
                            cancellationToken
                        );

                    voucherCustomer = await unitOfWork
                        .DynamicReadOnlyRepository<VoucherCustomer>()
                        .FindByConditionAsync(
                            new GetIsUsedVoucherCustomerSpecification(
                                order.CustomerId.Value,
                                getVoucher.Id
                            ),
                            cancellationToken
                        );

                    if (voucherCustomer == null)
                    {
                        await unitOfWork.RollbackAsync(cancellationToken);
                        return Result<CreateOrderResponse>.Failure(
                            new BadRequestError(
                                "Voucher is invalid!",
                                Messager
                                    .Create<Order>("Đơn hàng")
                                    .Property(x => x.VoucherId)
                                    .Message(MessageType.Existence)
                                    .Build()
                            )
                        );
                    }
                    else
                    {
                        var voucher = await unitOfWork
                            .DynamicReadOnlyRepository<Voucher>()
                            .FindByConditionAsync(
                                new GetVoucherWithIncludeByIdSpecification(getVoucher.Id),
                                x => x.ToGetVoucherDetailResponse(),
                                cancellationToken
                            );
                        discountApply = voucher.DiscountValue;
                        if (voucher.DiscountFixed)
                        {
                            order.Total = order.Amount - voucher.DiscountValue;
                        }
                        else
                        {
                            discountApply = (voucher.DiscountValue * order.Amount) / 100;
                            order.Total = order.Amount - discountApply;
                        }
                        voucherCustomer.IsUsed = true;
                    }
                }
                orderRes.DiscountValue = discountApply;
                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);

                CreateOrderResponse? newOrder = await unitOfWork
                    .DynamicReadOnlyRepository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(orderRes.Id),
                        x => x.ToCreateOrderResponse(),
                        cancellationToken
                    );

                return Result<CreateOrderResponse>.Success(newOrder!);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
