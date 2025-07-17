using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

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
            var point = await unitOfWork
                .Repository<Transaction>()
                .QueryAsync()
                .Where(x => x.CustomerId == currentAccount.Id && x.Type == TransactionType.Point)
                .SumAsync(x => x.Amount, cancellationToken);

            return Result<PointCustomerResponse>.Success(
                new PointCustomerResponse { Point = point }
            );
        }
    }
}
