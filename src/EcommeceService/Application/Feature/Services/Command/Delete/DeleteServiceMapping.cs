using Application.Feature.Common.Projections.Services;
using AutoMapper;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Delete;

public class DeleteServiceMapping : Profile
{
    public DeleteServiceMapping()
    {
        CreateMap<ServiceDeleteModel, Service>();
    }
}
