using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Queries.Detail;

public class GetOrderDetailHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<GetOrderDetailQuery, Result<GetOrderDetailResponse>>
{
    public async ValueTask<Result<GetOrderDetailResponse>> Handle(
        GetOrderDetailQuery request,
        CancellationToken cancellationToken
    )
    {
        OrderBranchReference? orderBranch = await unitOfWork
            .Repository<Order>()
            .FindByConditionAsync(
                x => x.Id == request.OrderId,
                x => new OrderBranchReference(x.BranchId),
                cancellationToken
            );
        if (orderBranch is null)
            return NotFound();

        if (
            !OrderBranchAccess
                .FromSession(currentAccount.Session?.Branches)
                .IsAuthorized(orderBranch.BranchId)
        )
            return Result<GetOrderDetailResponse>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        GetOrderDetailResponse? order = await unitOfWork
            .DynamicReadOnlyRepository<Order>()
            .FindByConditionAsync(
                new GetOrderByIdSpecification(request.OrderId),
                o => o.ToOrderDetailResponse(),
                cancellationToken
            );

        return order is null
            ? NotFound()
            : Result<GetOrderDetailResponse>.Success(order);
    }

    private static Result<GetOrderDetailResponse> NotFound() =>
        Result<GetOrderDetailResponse>.Failure(
            new NotFoundError(
                "Order not found",
                Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()
            )
        );
}
