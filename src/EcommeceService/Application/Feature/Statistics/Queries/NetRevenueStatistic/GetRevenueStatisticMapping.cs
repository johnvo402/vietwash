using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Queries.List;
using AutoMapper;
using Domain.Aggregates.Orders;

namespace Application.Feature.Statistics.Queries.RevenueStatistic;

public class GetRevenueStatisticMapping : Profile
{
    public GetRevenueStatisticMapping()
    {
        CreateMap<Order, ListOrderResponse>().IncludeBase<Order, OrderProjection>();
    }
}
