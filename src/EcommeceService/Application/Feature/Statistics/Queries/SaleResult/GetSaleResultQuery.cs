using Application.Feature.Statistics.Queries.SaleResult;
using Mediator;

namespace Application.Feature.Statistics.Queries.RevenueStatistic
{
    public class GetSaleResultQuery : IRequest<IEnumerable<GetSaleResultResponse>>;
}
