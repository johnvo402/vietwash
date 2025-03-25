using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Queries.Detail;
using Application.Feature.Statistics.Queries.TopService;
using AutoMapper;
using Domain.Aggregates.Orders;

public class GetTopServiceMapping : Profile
{
    public GetTopServiceMapping()
    {
        //CreateMap<Order, GetTopServiceResponse>()
        //    .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.OrderItems.FirstOrDefault().ServiceId))
        //    .ForMember(dest => dest.ServiceName, opt => opt.Ignore())
        //    .ForMember(dest => dest.Description, opt => opt.Ignore())
        //    .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
        //    .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Amount));
        CreateMap<Order, GetOrderDetailResponse>()
            .IncludeBase<Order, OrderDetailProjection>();
    }
}
