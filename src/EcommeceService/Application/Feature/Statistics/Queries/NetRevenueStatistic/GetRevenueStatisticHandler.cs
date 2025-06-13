using System.Data.Common;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Mediator;
using Npgsql;

public class GetRevenueStatisticHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<GetRevenueStatisticQuery, IEnumerable<GetRevenueStatisticResponse>>
{
    public async ValueTask<IEnumerable<GetRevenueStatisticResponse>> Handle(
        GetRevenueStatisticQuery request,
        CancellationToken cancellationToken
    )
    {
        var from = DateTime.Parse(request.From).ToString("yyyy-MM-dd");
        var to = DateTime.Parse(request.To).ToString("yyyy-MM-dd");

        string sql =
            @"
            SELECT revenue_date AS ""date"", total_revenue AS ""revenue""
            FROM get_revenue_statistics(@branchId, @from, @to);
        ";

        var parameters = new List<NpgsqlParameter>
        {
            new("@branchId", long.Parse(request.BranchId)),
            new("@from", DateOnly.Parse(from)),
            new("@to", DateOnly.Parse(to)),
        };

        var result = await unitOfWork.ExecuteSqlQueryAsync<GetRevenueStatisticResponse>(
            sql,
            parameters,
            cancellationToken
        );

        return result;
    }
}
