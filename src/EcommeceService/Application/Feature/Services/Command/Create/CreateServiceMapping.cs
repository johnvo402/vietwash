using Application.Feature.Common.Projections.Services;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Create
{
    public class CreateServiceMapping : Profile
    {
        public CreateServiceMapping()
        {
            CreateMap<CreateServiceCommand, Service>();
        }
    }
}
