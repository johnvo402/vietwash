using Application.Feature.Common.Projections.Services;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Common.Mapping.Services
{
    public class ServiceMapping : Profile
    {
        public ServiceMapping()
        {
            CreateMap<Service, ServiceProjection>();
            CreateMap<Service, ServiceDetailProjection>();
            CreateMap<Service, ServiceModel>();
        }
    }
}
