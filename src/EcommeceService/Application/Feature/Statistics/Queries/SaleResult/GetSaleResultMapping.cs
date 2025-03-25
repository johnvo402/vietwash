using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Queries.List;
using AutoMapper;
using Domain.Aggregates.Orders;

namespace Application.Feature.Statistics.Queries.SaleResult;

public class GetSaleResultMapping : Profile
{
    public GetSaleResultMapping()
    {
        CreateMap<Order, ListOrderResponse>().IncludeBase<Order, OrderProjection>();
    }
}
