using System.Data.Common;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Domain.Functions;
using Mediator;
using Microsoft.EntityFrameworkCore;

public class GetRevenueStatisticHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<GetRevenueStatisticQuery, IEnumerable<GetRevenueStatistic>>
{
    public async ValueTask<IEnumerable<GetRevenueStatistic>> Handle(
        GetRevenueStatisticQuery request,
        CancellationToken cancellationToken
    )
    {
        var from = DateTime.Parse(request.From).ToString("yyyy-MM-dd");
        var to = DateTime.Parse(request.To).ToString("yyyy-MM-dd");

        var parameters = new object[]
        {
            long.Parse(request.BranchId),
            DateOnly.Parse(from),
            DateOnly.Parse(to),
        };

        var result = await unitOfWork
            .CallPostgreSqlFunction<GetRevenueStatistic>(
                functionName: "get_revenue_statistics",
                parameters
            )
            .ToListAsync(cancellationToken);

        return result;
    }
}
