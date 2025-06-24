using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using Mediator;

namespace Application.Features.Funds.Queries.Detail
{
    public class GetFundDetailHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetFundDetailQuery, Result<GetFundDetailResponse>>
    {
        public async ValueTask<Result<GetFundDetailResponse>> Handle(
            GetFundDetailQuery request,
            CancellationToken cancellationToken
        )
        {
            GetFundDetailResponse? fund = await unitOfWork
                .DynamicReadOnlyRepository<Fund>()
                .FindByConditionAsync(
                    new GetFundByIdSpecification(request.FundId),
                    x => x.ToGetFundDetailResponse(),
                    cancellationToken
                );
            if (fund == null)
            {
                return Result<GetFundDetailResponse>.Failure(
                    new NotFoundError(
                        "Fund not found",
                        Messager.Create<Fund>().Message(MessageType.Found).Negative().BuildMessage()
                    )
                );
            }

            return Result<GetFundDetailResponse>.Success(fund);
        }
    }
}
