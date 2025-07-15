using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Queries.DetailByCode
{
    public class GetOrderDetailByCodeHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetOrderDetailByCodeQuery, Result<GetOrderDetailByCodeResponse>>
    {
        public async ValueTask<Result<GetOrderDetailByCodeResponse>> Handle(
            GetOrderDetailByCodeQuery request,
            CancellationToken cancellationToken
        )
        {
            var order = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .FindByConditionAsync(
                    new GetOrderByCodeSpecification(request.Code),
                    o => o.ToOrderDetailByCodeResponse(),
                    cancellationToken
                );
            if (order == null)
            {
                return Result<GetOrderDetailByCodeResponse>.Failure(
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

            return Result<GetOrderDetailByCodeResponse>.Success(order);
        }
    }
}
