using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Vouchers.Queries.CheckCode
{
    public class CheckCodeHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CheckCodeQuery, Result<CheckCodeResponse>>
    {
        public async ValueTask<Result<CheckCodeResponse>> Handle(
            CheckCodeQuery request,
            CancellationToken cancellationToken
        )
        {
            CheckCodeResponse? voucher = await unitOfWork
                .Repository<Voucher>()
                .QueryAsync(x => x.Code == request.VoucherCode)
                .Select(x => new CheckCodeResponse
                {
                    DiscountFixed = x.DiscountFixed,
                    DiscountValue = x.DiscountValue,
                })
                .FirstOrDefaultAsync();
            if (voucher == null)
            {
                return Result<CheckCodeResponse>.Failure(
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
            return Result<CheckCodeResponse>.Success(voucher);
        }
    }
}
