using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Orders.Queries.List
{
    public class ListOrderHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListOrderQuery, PaginationResponse<ListOrderResponse>>
    {
        public async ValueTask<PaginationResponse<ListOrderResponse>> Handle(
            ListOrderQuery query,
            CancellationToken cancellationToken
        ) =>
            await unitOfWork
                .Repository<Order>()
                .PagedListAsync<ListOrderResponse>(
                    new ListOrderSpecification(
                        DateTime.Parse(query.From),
                        DateTime.Parse(query.To)
                    ),
                    query.ValidateQuery().ValidateFilter(typeof(ListOrderResponse))
                );
    }
}
