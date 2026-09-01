using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Queries.DetailByCode;

public class GetOrderDetailByCodeHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryption,
    ICurrentAccount currentAccount
) : IRequestHandler<GetOrderDetailByCodeQuery, Result<GetOrderDetailByCodeResponse>>
{
    public async ValueTask<Result<GetOrderDetailByCodeResponse>> Handle(
        GetOrderDetailByCodeQuery request,
        CancellationToken cancellationToken
    )
    {
        string code = encryption.Decrypt(request.Code);
        OrderBranchReference? orderBranch = await unitOfWork
            .Repository<Order>()
            .FindByConditionAsync(
                x => x.Code == code,
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
            return Result<GetOrderDetailByCodeResponse>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        GetOrderDetailByCodeResponse? order = await unitOfWork
            .DynamicReadOnlyRepository<Order>()
            .FindByConditionAsync(
                new GetOrderByCodeSpecification(code),
                o => o.ToOrderDetailByCodeResponse(),
                cancellationToken
            );

        return order is null
            ? NotFound()
            : Result<GetOrderDetailByCodeResponse>.Success(order);
    }

    private static Result<GetOrderDetailByCodeResponse> NotFound() =>
        Result<GetOrderDetailByCodeResponse>.Failure(
            new NotFoundError(
                "Order not found",
                Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()
            )
        );
}
