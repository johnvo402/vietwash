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
            GetNetRevenueBranchQuery request,
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
                // Giả sử Amount là doanh thu cần tính
                var netRevenue = order.Total;

                if (revenueByBranch.TryGetValue(order.BranchId, out var existBranch))
                {
                    existBranch.TotalNetRevenue += netRevenue;
                }
                else
                {
                    revenueByBranch[order.BranchId] = new GetNetRevenueBranchResponse
                    {
                        BranchId = order.BranchId,
                        TotalNetRevenue = netRevenue,
                    };
                }
            }

            var totalRevenue = revenueByBranch.Values.Sum(x => x.TotalNetRevenue);
            if (totalRevenue > 0)
            {
                foreach (var branch in revenueByBranch.Values)
                {
                    branch.Percentage = (float)
                        Math.Round((branch.TotalNetRevenue / totalRevenue) * 100, 2);
                }
            }

            return Result<IEnumerable<GetNetRevenueBranchResponse>>.Success(revenueByBranch.Values);
        }
    }
}
