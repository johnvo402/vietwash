using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Update
{
    public class UpdateServiceMapping : Profile
    {
        public UpdateServiceMapping()
        {
            CreateMap<ServiceModel, Service>();

			CreateMap<Service, UpdateServiceResponse>()
                .IncludeBase<Service, ServiceDetailProjection>();
        }
    }
}
