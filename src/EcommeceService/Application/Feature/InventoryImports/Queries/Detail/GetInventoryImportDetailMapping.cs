using Application.Feature.Common.Projections.InventoryImports;
using Application.Feature.InventoryImports.Command.Create;
using Application.Feature.InventoryImports.Command.Update;
using AutoMapper;
using Domain.Aggregates.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Queries.Detail
{
    public class GetInventoryImportDetailMapping : Profile
    {
        public GetInventoryImportDetailMapping()
        {
            CreateMap<InventoryDocument, InventoryImportDetailProjection>();

            CreateMap<ProductSupplying, ProductSupplyingProjection>();
            CreateMap<EquipmentSupplying, EquipmentSupplyingProjection>();

            CreateMap<InventoryDocument, GetInventoryImportDetailResponse>()
                .IncludeBase<InventoryDocument, InventoryImportDetailProjection>()
                .ForMember(dest => dest.ProductSupplyings, opt => opt.MapFrom(src => src.ProductSupplyings))
                .ForMember(dest => dest.EquipmentSupplyings, opt => opt.MapFrom(src => src.EquipmentSupplyings));
        }
    }
}
