using Application.Feature.Common.Projections.Orders;
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
			CreateMap<OrderModel, Order>();
			CreateMap<OrderItemModel, OrderItem>();
			CreateMap<Order, OrderProjection>();
			CreateMap<Order, OrderDetailProjection>();
			CreateMap<OrderItem, OrderItemProjection>();
		}
	}
}
