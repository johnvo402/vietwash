using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Funds.Queries.List
{
    public class ListFundHandler(IUnitOfWork unitOfWork)
  : IRequestHandler<ListFundQuery, PaginationResponse<ListFundResponse>>
    {
        public async ValueTask<PaginationResponse<ListFundResponse>> Handle(ListFundQuery request, CancellationToken cancellationToken)
        {



            return await unitOfWork
                .Repository<Fund>()
                .PagedListAsync<ListFundResponse>(
                    new ListFundSpecification(request.From,
                        request.To),
                    request.ValidateQuery().ValidateFilter(typeof(ListFundResponse))
                );
        }
    }

}
