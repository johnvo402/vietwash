using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Orders.Queries.List
{
    public class ListOrderHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
        : IRequestHandler<ListOrderQuery, PaginationResponse<ListOrderResponse>>
    {
        public async ValueTask<PaginationResponse<ListOrderResponse>> Handle(
            ListOrderQuery query,
            CancellationToken cancellationToken
        )
        {
            var listBranchUser = currentUser.Session!.Branches!.ToList();
            return await unitOfWork
                .Repository<Order>()
                .PagedListAsync<ListOrderResponse>(
                    new ListOrderSpecification(
                        query.From,
                        query.To,
                        query.BranchId,
                        listBranchUser
                    ),
                    query.ValidateQuery().ValidateFilter(typeof(ListOrderResponse))
                );
        }
    }
}
