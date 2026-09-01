using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
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
                .QueryAsync(
                    VoucherEligibility.ForCustomer(
                        request.VoucherCode.Trim(),
                        request.CustomerId,
                        DateTimeOffset.UtcNow
                    )
                )
                .Select(x => new CheckCodeResponse
                {
                    DiscountFixed = x.DiscountFixed,
                    DiscountValue = x.DiscountValue,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (voucher is null)
            {
                return Result<CheckCodeResponse>.Failure(
                    new NotFoundError(
                        "Voucher is invalid, inactive, expired, used, or not assigned to this customer.",
                        Messager.Create<Voucher>().Message(MessageType.Valid).Negative().Build()
                    )
                );
            }

            return Result<CheckCodeResponse>.Success(voucher);
        }
    }
}
