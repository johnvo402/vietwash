using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Tariffs;
using AutoMapper;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Commands.Create
{
    public class CreateTariffMapping : Profile
    {
        public CreateTariffMapping()
        {
            CreateMap<TariffModel, Tariff>();
        }
    }
}