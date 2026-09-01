using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Feature.Orders.Queries.List;

public class ListOrderHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<ListOrderQuery, Result<PaginationResponse<ListOrderResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListOrderResponse>>> Handle(
        ListOrderQuery query,
        CancellationToken cancellationToken
    )
    {
        Result<PaginationResponse<ListOrderResponse>>? validation =
            query.Validate<ListOrderQuery, ListOrderResponse>();
        if (validation is not null)
            return validation;

        OrderBranchAccess branchAccess = OrderBranchAccess.FromSession(
            currentUser.Session?.Branches
        );
        long? requestedBranchId = null;
        if (!string.IsNullOrWhiteSpace(query.BranchId))
        {
            if (
                !long.TryParse(query.BranchId, out long parsedBranchId)
                || parsedBranchId <= 0
            )
            {
                return Result<PaginationResponse<ListOrderResponse>>.Failure(
                    new BadRequestError(
                        "BranchId invalid",
                        Messager.Create<Order>().Message(MessageType.Valid).Negative().BuildMessage()
                    )
                );
            }

            requestedBranchId = parsedBranchId;
            if (!branchAccess.IsAuthorized(parsedBranchId))
                return Result<PaginationResponse<ListOrderResponse>>.Failure(
                    new ForbiddenError(Message.FORBIDDEN)
                );
        }

        long? customerId = currentUser.Session?.Role == ROLE.CUSTOMER
            ? currentUser.Id
            : null;
        PaginationResponse<ListOrderResponse> response = await unitOfWork
            .DynamicReadOnlyRepository<Order>(false)
            .PagedListAsync(
                new ListOrderSpecification(
                    query.From,
                    query.To,
                    requestedBranchId,
                    branchAccess.BranchIds,
                    customerId: customerId
                ),
                query,
                ListOrderMapping.Selector(),
                cancellationToken: cancellationToken
            );

        return Result<PaginationResponse<ListOrderResponse>>.Success(response);
    }
}
