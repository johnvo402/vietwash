using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FundBehaviors.Queries
{

    public class ListFundBehaviorHandler(IUnitOfWork unitOfWork)
     : IRequestHandler<ListFundBehaviorQuery, IEnumerable<ListFundBehaviorResponse>>
    {
        public async ValueTask<IEnumerable<ListFundBehaviorResponse>> Handle(
            ListFundBehaviorQuery query,
            CancellationToken cancellationToken
        ) =>
            await unitOfWork
                .CachedRepository<FundBehavior>()
                .ListAsync<ListFundBehaviorResponse>(cancellationToken);
    }
}
