using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Mediator;

namespace Application.Feature.Vouchers.Commands.Update
{
    public class UpdateVoucherHandler(
        IUnitOfWork unitOfWork,
        IMediaUpdateService mediaUpdateService,
        IQrGenerator qrGenerator
    ) : IRequestHandler<UpdateVoucherCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateVoucherCommand command,
            CancellationToken cancellationToken
        )
        {
            Voucher? existingVoucher = await unitOfWork
                .DynamicReadOnlyRepository<Voucher>()
                .FindByConditionAsync(
                    new GetVoucherWithIncludeByIdSpecification(command.VoucherId),
                    cancellationToken
                );
            if (existingVoucher == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Voucher not found",
                        Messager
                            .Create<Voucher>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            var barcode = qrGenerator.GenerateQrBase64(command.Voucher.Code!);
            string? oldVoucherImage = existingVoucher.ImgUrl;

            existingVoucher.FromUpdateModel(command.Voucher, barcode);

            string? newVoucherImage = command.Voucher.ImgUrl;
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Voucher>().UpdateAsync(existingVoucher);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(oldVoucherImage))
                {
                    await mediaUpdateService.DeleteMediaAsync(oldVoucherImage);
                }

                return Result.Success();
            }
            catch
            {
                if (!string.IsNullOrEmpty(newVoucherImage))
                {
                    await mediaUpdateService.DeleteMediaAsync(newVoucherImage);
                }
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
