using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.Create;
using AutoMapper;
using Domain.Aggregates.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Mapping.Orders
{
	public class OrderMapping : Profile
	{
		public OrderMapping() 
		{
			CreateMap<CreateOrderCommand, Order>();
			CreateMap<CreateOrderItemModel, OrderItem>();
			CreateMap<Order, OrderProjection>();
			CreateMap<Order, OrderDetailProjection>();
			CreateMap<OrderItem, OrderItemProjection>();
		}
	}
}
