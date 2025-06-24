using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using Mediator;

namespace Application.Features.FundBehaviors.Queries
{
    public class ListFundBehaviorHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListFundBehaviorQuery, Result<IEnumerable<ListFundBehaviorResponse>>>
    {
        public async ValueTask<Result<IEnumerable<ListFundBehaviorResponse>>> Handle(
            ListFundBehaviorQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.ValidateWithoutPaging<
                ListFundBehaviorQuery,
                IEnumerable<ListFundBehaviorResponse>
            >();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<FundBehavior>()
                .ListAsync(
                    new ListFundBehaviorSpecification(),
                    query,
                    ListFundBehaviorMapping.Selector(),
                    cancellationToken
                );
            return Result<IEnumerable<ListFundBehaviorResponse>>.Success(response);
        }
    }
}
