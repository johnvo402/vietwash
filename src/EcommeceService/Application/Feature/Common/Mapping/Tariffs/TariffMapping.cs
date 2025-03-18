using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Tariffs;
using AutoMapper;
using Domain.Aggregates.Tariffs;


namespace Application.Feature.Common.Mapping.Tariffs
{
    public class TariffMapping : Profile
    {
        public TariffMapping()
        {
            CreateMap<Tariff, TariffProjection>();
            CreateMap<Tariff, TariffDetailProjection>();
        }
    }
}