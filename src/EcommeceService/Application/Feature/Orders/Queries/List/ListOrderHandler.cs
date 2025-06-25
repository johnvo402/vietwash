using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Queries.List
{
    public class ListOrderHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
        : IRequestHandler<ListOrderQuery, Result<PaginationResponse<ListOrderResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListOrderResponse>>> Handle(
            ListOrderQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.Validate<ListOrderQuery, ListOrderResponse>();

            if (validation != null)
            {
                return validation;
            }

            var listBranchUser = currentUser.Session!.Branches!.ToList();
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Order>(false)
                .CursorPagedListAsync(
                    new ListOrderSpecification(
                        query.From,
                        query.To,
                        query.BranchId,
                        listBranchUser
                    ),
                    query,
                    ListOrderMapping.Selector(),
                    cancellationToken: cancellationToken
                );

            return Result<PaginationResponse<ListOrderResponse>>.Success(response);
        }
    }
}
