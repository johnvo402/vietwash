
using Application.Feature.Common.Projections.Services;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Queries.List;

public class ListServiceMapping : Profile
{
    public ListServiceMapping()
    {
        CreateMap<Service, ServiceProjection>();
        CreateMap<Service, ListServiceResponse>().IncludeBase<Service, ServiceProjection>();

    }
}
