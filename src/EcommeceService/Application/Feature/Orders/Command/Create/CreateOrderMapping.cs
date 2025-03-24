using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Queries.Detail;
using AutoMapper;
using Domain.Aggregates.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Create
{
	public class CreateOrderMapping : Profile
	{
		public CreateOrderMapping()
		{
			CreateMap<CreateOrderCommand, Order>()
				.ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => Ulid.Parse(src.CustomerId)))
				.ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src =>
					src.OrderItems.Select(item => new OrderItem
					{
						ServiceId = Ulid.Parse(item.ServiceId),
						UnitRelationId = Ulid.Parse(item.UnitRelationId),
						Price = item.Price
					}).ToList()));

			CreateMap<Order, CreateOrderResponse>().IncludeBase<Order, OrderDetailProjection>();
		}
	}
}
