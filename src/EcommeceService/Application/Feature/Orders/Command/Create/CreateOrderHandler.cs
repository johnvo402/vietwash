using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.Detail;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
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

                VoucherUsage? voucherUsage = null;

                if (order.CustomerId != null && order.VoucherId != null)
                {
                    voucherUsage = await unitOfWork
                        .DynamicReadOnlyRepository<VoucherUsage>()
                        .FindByConditionAsync(
                            new GetUsageVoucherSpecification(
                                order.VoucherId!.Value,
                                order.CustomerId!.Value
                            ),
                            cancellationToken
                        );

                    if (voucherUsage != null)
                    {
                        await unitOfWork.RollbackAsync(cancellationToken);
                        return Result<CreateOrderResponse>.Failure(
                            new BadRequestError(
                                "Voucher has been used!",
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
                        //                        User? user = await unitOfWork
                        //.DynamicReadOnlyRepository<User>()
                        //.FindByConditionAsync(
                        //new GetUserByIdWithoutIncludeSpecification(order.CustomerId.Value),
                        //cancellationToken
                        //);
                        //                        short customerGroupValue = (short)user.CustomerGroup.Value;

                        //                        var voucher = await unitOfWork
                        //                            .DynamicReadOnlyRepository<Voucher>()
                        //                            .FindByConditionAsync(
                        //                                new GetVoucherByCustomerSpecification(order.VoucherId.Value, customerGroupValue),
                        //                                x => x.ToGetVoucherDetailResponse(),
                        //                                cancellationToken
                        //                            );
                        var voucher = await unitOfWork
                            .DynamicReadOnlyRepository<Voucher>()
                            .FindByConditionAsync(
                                new GetVoucherWithIncludeByIdSpecification(order.VoucherId.Value),
                                x => x.ToGetVoucherDetailResponse(),
                                cancellationToken
                            );

                        if (voucher.DiscountFixed)
                        {
                            order.Total = order.Amount - voucher.DiscountValue;
                        }
                        else
                        {
                            order.Total = order.Amount - (voucher.DiscountValue * order.Amount) / 100;
                        }
                    }
                }

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                if (order.VoucherId != null && voucherUsage == null)
                {
                    order.EmitVoucherUsageEvent();
                    await unitOfWork.SaveAsync(cancellationToken);
                }
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
