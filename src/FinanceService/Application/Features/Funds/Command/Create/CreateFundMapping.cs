using AutoMapper;
using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Command.Create
{
    public class CreateFundMapping : Profile
    {
        public CreateFundMapping()
        {

            CreateMap<CreateFundCommand, Fund>();

        }
    }
}
