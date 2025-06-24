using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.BranchNetRevenue
{
    public class GetNetRevenueBranchHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
        : IRequestHandler<
            GetNetRevenueBranchQuery,
            Result<IEnumerable<GetNetRevenueBranchResponse>>
        >
    {
        public async ValueTask<Result<IEnumerable<GetNetRevenueBranchResponse>>> Handle(
            [FromQuery] GetNetRevenueBranchQuery request,
            CancellationToken cancellationToken
        )
        {
            var queryParamRequest = new QueryParamRequest();
            var listBranchUser = currentUser.Session!.Branches!.ToList();
            var orders = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .ListAsync(
                    new ListOrderSpecification(request.From, request.To, null, listBranchUser),
                    queryParamRequest,
                    cancellationToken
                );

            var revenueByBranch = new Dictionary<long, GetNetRevenueBranchResponse>();

            foreach (var order in orders)
            {
                if (revenueByBranch.TryGetValue(order.BranchId, out var existBranch))
                {
                    existBranch.TotalNetRevenue += order.Amount;
                }
                else
                {
                    revenueByBranch[order.BranchId] = new GetNetRevenueBranchResponse
                    {
                        BranchId = order.BranchId,
                        TotalNetRevenue = order.Amount,
                    };
                }
            }

            var totalRevenue = revenueByBranch.Values.Sum(x => x.TotalNetRevenue);
            if (totalRevenue > 0)
            {
                foreach (var branch in revenueByBranch.Values)
                {
                    branch.Percentage =
                        (float)Math.Ceiling((branch.TotalNetRevenue / totalRevenue) * 100 * 100)
                        / 100;
                }
            }
            return Result<IEnumerable<GetNetRevenueBranchResponse>>.Success(revenueByBranch.Values);
        }
    }
}
