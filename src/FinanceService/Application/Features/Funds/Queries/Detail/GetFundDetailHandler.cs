using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;




namespace Application.Features.Funds.Queries.Detail
{
    public class GetFundDetailHandler(IUnitOfWork unitOfWork)
          : IRequestHandler<GetFundDetailQuery, GetFundDetailResponse>
    {
        public async ValueTask<GetFundDetailResponse> Handle(
            GetFundDetailQuery request,
            CancellationToken cancellationToken
        ) =>
            await unitOfWork
                .Repository<Fund>()
                .FindByConditionAsync<GetFundDetailResponse>(
                    new GetFundByIdSpecification(long.Parse(request.FundId)),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Fund>().Message(MessageType.Found).Negative().BuildMessage()]
            );
    }
}
