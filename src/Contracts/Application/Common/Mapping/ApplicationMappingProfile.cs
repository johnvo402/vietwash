using AutoMapper;
using Contracts.Dtos.Requests;

namespace Application.Common.Mapping;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<string, Ulid>().ConvertUsing(src => Ulid.Parse(src));
        CreateMap<MessageMailMetaData, MailData>()
            .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Message));

        CreateMap<TemplateMailMetaData, MailData>()
            .ForMember(
                dest => dest.Body,
                opt => opt.MapFrom((src, dest, member, context) =>
                    context.Items.ContainsKey("body") ? context.Items["body"] : src.DisplayName
                )
            );

    }
}
