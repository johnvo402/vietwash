using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Customers.Queries.Detail;

public class GetCustomerDetailHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCustomerDetailQuery, Result<GetCustomerDetailResponse>>
{
    public async ValueTask<Result<GetCustomerDetailResponse>> Handle(
        GetCustomerDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        GetCustomerDetailResponse? customer = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdSpecification(query.AccountId),
                x => x.ToGetCustomerDetailResponse(),
                cancellationToken
            );
        if (customer == null)
        {
            return Result<GetCustomerDetailResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }
        return Result<GetCustomerDetailResponse>.Success(customer);
    }
}
