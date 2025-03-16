using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Tariffs;
using AutoMapper;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffMapping : Profile
    {
        public UpdateTariffMapping()
        {
            CreateMap<UpdateTariff, Tariff>();
            CreateMap<Tariff, UpdateTariffResponse>().IncludeBase<Tariff, TariffDetailProjection>();
        }
    }
}