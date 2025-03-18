using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Tariffs;
using AutoMapper;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Queries
{
    public class ListTariffMapping : Profile
    {
        public ListTariffMapping()
        {
            CreateMap<Tariff, TariffProjection>();
            CreateMap<Tariff, ListTariffResponse>().IncludeBase<Tariff, TariffProjection>();
        }
    }
}