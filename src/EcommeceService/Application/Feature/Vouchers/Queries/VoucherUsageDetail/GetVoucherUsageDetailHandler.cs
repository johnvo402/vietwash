using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.Detail;
using Contracts.Common.Messages;
using Domain.Aggregates.Vouchers.Specifications;
using Mediator;
using Contracts.ApiWrapper;

namespace Application.Feature.Vouchers.Queries.VoucherUsageDetail
{
    public class GetVoucherUsageDetailHandler(IUnitOfWork unitOfWork)
     : IRequestHandler<GetVoucherUsageDetailQuery, Result<GetVoucherUsageDetailResponse>>
    {
        public async ValueTask<Result<GetVoucherUsageDetailResponse>> Handle(
            GetVoucherUsageDetailQuery command,
            CancellationToken cancellationToken
        )
        {
            var usage = await unitOfWork
                .DynamicReadOnlyRepository<Domain.Aggregates.Vouchers.VoucherUsage>()
                .FindByConditionAsync(
                    new GetVoucherUsageDetailByIdSpecification(command.VoucherUsageId),
                    x => x.ToGetVoucherUsageDetailResponse(),
                    cancellationToken
                );

            if (usage is null)
            {
                return Result<GetVoucherUsageDetailResponse>.Failure(
                    new NotFoundError(
                        "VoucherUsage not found",
                        Messager
                            .Create<Domain.Aggregates.Vouchers.VoucherUsage>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            return Result<GetVoucherUsageDetailResponse>.Success(usage);
        }
    }

}
