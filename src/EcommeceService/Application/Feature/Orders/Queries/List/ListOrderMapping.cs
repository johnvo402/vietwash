using System;
using System.Collections.Generic;
using Application.Feature.Common.Projections.Orders;
using AutoMapper;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.List
{
    public class ListOrderMapping : Profile
    {
        public ListOrderMapping()
        {
            CreateMap<Order, ListOrderResponse>().IncludeBase<Order, OrderProjection>();
        }
    }
}
