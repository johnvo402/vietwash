using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Common.Messages;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Mediator;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderHandler(
        IUnitOfWork unitOfWork,
        ICurrentAccount _currentAccount,
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
            Order order = request.ToEntity((long)_currentAccount.Id!, orgSetting.VatPercent);
            var codeEncrypt = encryption.Encrypt(order.Code);
            var barcodeConfirm = barcode.GenerateQrBase64(codeEncrypt);
            order.CodeConfirm = barcodeConfirm;
            Voucher? getVoucher = null;
            if (order.CustomerId != null && order.VoucherCode != null)
            {
                getVoucher = await unitOfWork
                    .DynamicReadOnlyRepository<Voucher>()
                    .FindByConditionAsync(
                        new GetVoucherByCodeSpecification(
                            order.VoucherCode,
                            order.CustomerId.Value
                        ),
                        cancellationToken
                    );

                if (getVoucher == null)
                {
                    return Result<CreateOrderResponse>.Failure(
                        new NotFoundError(
                            "Voucher not found!",
                            Messager.Create<Voucher>().Message(MessageType.Existence).Build()
                        )
                    );
                }
                order.DiscountValue = getVoucher.DiscountValue;
                order.DiscountFixed = getVoucher.DiscountFixed;
                foreach (var voucherCus in getVoucher.VoucherCustomers)
                {
                    voucherCus.IsUsed = true;
                }
            }
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                var orderRes = await unitOfWork
                    .Repository<Order>()
                    .AddAsync(order, cancellationToken);
                if (getVoucher != null)
                {
                    await unitOfWork.Repository<Voucher>().UpdateAsync(getVoucher);
                }

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
