using AutoMapper;
using Domain.Aggregates.PubSubLogs;

namespace Application.Features.PubSubLogs;

public class CreatePubSubLogMapper : Profile
{
    public CreatePubSubLogMapper()
    {
        CreateMap<CreatePubSubLogCommand, PubSubLog>();
    }
}
