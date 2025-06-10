using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Queries.Detail;
using AutoMapper;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderMapping : Profile
    {
        public CreateOrderMapping()
        {
            CreateMap<CreateOrderCommand, Order>()
                .ForMember(
                    dest => dest.CustomerId,
                    opt => opt.MapFrom(src => Ulid.Parse(src.CustomerId))
                )
                .ForMember(
                    dest => dest.OrderItems,
                    opt =>
                        opt.MapFrom(src =>
                            src.OrderItems.Select(item => new OrderItem
                            {
                                ServiceId = item.ServiceId,
                                UnitRelationId = item.UnitRelationId,
                                Quantity = item.Quantity,
                                Price = item.Price,
                            })
                                .ToList()
                        )
                );

            CreateMap<Order, CreateOrderResponse>().IncludeBase<Order, OrderDetailProjection>();
            CreateMap<GetOrderDetailResponse, CreateOrderResponse>();
        }
    }
}
