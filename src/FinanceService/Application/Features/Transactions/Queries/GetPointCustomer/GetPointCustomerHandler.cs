using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Transactions;
using Contracts.ApiWrapper;
using Domain.Aggregates.Funds;
using Mediator;

namespace Application.Features.Transactions.Queries.GetPointCustomer
{
    public class GetPointCustomerHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetPointCustomerQuery, Result<GetPointCustomerResponse>>
    {
        public async ValueTask<Result<GetPointCustomerResponse>> Handle(
            GetPointCustomerQuery request,
            CancellationToken cancellationToken
        )
        {
            var point = await PointLedger.GetBalanceAsync(
                unitOfWork.Repository<Transaction>().QueryAsync(),
                request.CustomerId,
                cancellationToken
            );

            return Result<GetPointCustomerResponse>.Success(
                new GetPointCustomerResponse { Point = point }
            );
        }
    }
}
