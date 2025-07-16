using System.Data.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

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
