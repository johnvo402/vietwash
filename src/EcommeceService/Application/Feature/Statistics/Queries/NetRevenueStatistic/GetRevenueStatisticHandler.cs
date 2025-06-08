using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Mediator;

public class GetRevenueStatisticHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRevenueStatisticQuery, IEnumerable<GetRevenueStatisticResponse>>
{
    public async ValueTask<IEnumerable<GetRevenueStatisticResponse>> Handle(
        GetRevenueStatisticQuery request,
        CancellationToken cancellationToken
    )
    {
        var from = DateTime.Parse(request.From).ToString("yyyy-MM-dd");
        var to = DateTime.Parse(request.To).ToString("yyyy-MM-dd");
        var query = $@"
    SELECT revenue_date AS ""Date"", total_revenue AS ""Revenue""
    FROM get_revenue_statistics('{request.BranchId}', '{from}', '{to}');
";


        var result = await unitOfWork.ExecuteSqlQueryAsync<GetRevenueStatisticResponse>(
            query,
            cancellationToken
        );
        return result;
    }
}
