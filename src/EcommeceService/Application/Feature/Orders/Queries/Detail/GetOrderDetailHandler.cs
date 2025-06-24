using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Queries.Detail
{
    public class GetOrderDetailHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetOrderDetailQuery, Result<GetOrderDetailResponse>>
    {
        public async ValueTask<Result<GetOrderDetailResponse>> Handle(
            GetOrderDetailQuery request,
            CancellationToken cancellationToken
        )
        {
            var order = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .FindByConditionAsync(
                    new GetOrderByIdSpecification(request.OrderId),
                    cancellationToken
                );
            if (order == null)
            {
                return Result<GetOrderDetailResponse>.Failure(
                    new NotFoundError(
                        "Order not found",
                        Messager
                            .Create<Order>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            var response = order.ToOrderDetailResponse();
            return Result<GetOrderDetailResponse>.Success(response);
        }
    }
}
