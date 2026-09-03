using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Rules;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Infrastructure.Constants;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Vouchers.Queries.CheckCode
{
    public class CheckCodeHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
        : IRequestHandler<CheckCodeQuery, Result<CheckCodeResponse>>
    {
        public async ValueTask<Result<CheckCodeResponse>> Handle(
            CheckCodeQuery request,
            CancellationToken cancellationToken
        )
        {
            // Authorize ownership before even querying the target user or voucher.
            var role = currentAccount.Session?.Role;
            if (
                !OrderActorAccess.IsStaffSide(role)
                && !(role == ROLE.CUSTOMER && currentAccount.Id == request.CustomerId)
            )
                return Result<CheckCodeResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

            if (
                !await unitOfWork
                    .Repository<User>()
                    .AnyAsync(CustomerEligibility.ForId(request.CustomerId), cancellationToken)
            )
                return Result<CheckCodeResponse>.Failure(
                    new NotFoundError(
                        "Active customer not found.",
                        Messager.Create<User>().Message(MessageType.Existence).Negative().Build()
                    )
                );

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
