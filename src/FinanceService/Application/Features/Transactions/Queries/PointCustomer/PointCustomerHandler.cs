using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Transactions;
using Contracts.ApiWrapper;
using Domain.Aggregates.Funds;
using Mediator;

namespace Application.Features.Transactions.Queries.PointCustomer
{
    public class PointCustomerHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
        : IRequestHandler<PointCustomerQuery, Result<PointCustomerResponse>>
    {
        public async ValueTask<Result<PointCustomerResponse>> Handle(
            PointCustomerQuery request,
            CancellationToken cancellationToken
        )
        {
            var point = await PointLedger.GetBalanceAsync(
                unitOfWork.Repository<Transaction>().QueryAsync(),
                currentAccount.Id ?? 0,
                cancellationToken
            );

            return Result<PointCustomerResponse>.Success(
                new PointCustomerResponse { Point = point }
            );
        }
    }
}
