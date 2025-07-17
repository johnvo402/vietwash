using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.Detail;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers.Specifications;
using Domain.Aggregates.Vouchers;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.ApiWrapper;

namespace Application.Feature.Vouchers.Queries.Detail
{
    public class GetVoucherDetailHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetVoucherDetailQuery, Result<GetVoucherDetailResponse>>
    {
        public async ValueTask<Result<GetVoucherDetailResponse>> Handle(
            GetVoucherDetailQuery command,
            CancellationToken cancellationToken
        )
        {
            GetVoucherDetailResponse? voucher = await unitOfWork
                .DynamicReadOnlyRepository<Voucher>()
                .FindByConditionAsync(
                    new GetVoucherWithIncludeByIdSpecification(command.VoucherId),
                    x => x.ToGetVoucherDetailResponse(),
                    cancellationToken
                );
            if (voucher == null)
            {
                return Result<GetVoucherDetailResponse>.Failure(
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

            return Result<GetVoucherDetailResponse>.Success(voucher);
        }
    }
}
