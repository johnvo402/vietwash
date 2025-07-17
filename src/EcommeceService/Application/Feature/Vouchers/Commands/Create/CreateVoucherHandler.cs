using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Vouchers;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Vouchers.Commands.Create
{
    public class CreateVoucherHandler(
        IUnitOfWork unitOfWork,
        IMediaUpdateService mediaUpdateService
    ) : IRequestHandler<CreateVoucherCommand, Result>
    {
        public async ValueTask<Result> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
        {

            Voucher mappingVoucher = request.ToEntity();

            string? voucherImage = null;
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                Voucher voucher = await unitOfWork
                    .Repository<Voucher>()
                    .AddAsync(mappingVoucher, cancellationToken);
                voucherImage = voucher.ImgUrl;

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(voucherImage))
                {
                    await mediaUpdateService.DeleteAvatarAsync(voucherImage);
                }
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
