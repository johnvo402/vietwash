using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Rules;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Queries.Detail;

public sealed class GetCustomerHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<GetCustomerQuery, Result<GetCustomerResponse>>
{
    public async ValueTask<Result<GetCustomerResponse>> Handle(
        GetCustomerQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!OrderActorAccess.IsStaffSide(currentAccount.Session?.Role))
            return Result<GetCustomerResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

        var customer = await unitOfWork
            .Repository<User>()
            .FindByConditionAsync(
                CustomerEligibility.ForId(request.Id),
                x => new GetCustomerResponse
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    CustomerGroup = x.CustomerGroup,
                    Status = x.Status,
                    Role = x.Role,
                },
                cancellationToken
            );
        return customer is not null
            ? Result<GetCustomerResponse>.Success(customer)
            : Result<GetCustomerResponse>.Failure(
                new NotFoundError(
                    "Active customer not found.",
                    Messager.Create<User>().Message(MessageType.Existence).Negative().Build()
                )
            );
    }
}
