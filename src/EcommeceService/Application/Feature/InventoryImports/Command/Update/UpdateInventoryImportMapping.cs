using Application.Feature.Common.Projections.InventoryImports;
using Application.Feature.InventoryImports.Command.Create;
using AutoMapper;
using Domain.Aggregates.Inventories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Command.Update
{
    public class UpdateInventoryImportMapping : Profile
    {
        public UpdateInventoryImportMapping()
        {
            CreateMap<InventoryImportModel, InventoryDocument>();

            CreateMap<CreateInventoryImportCommand, InventoryDocument>()
                .IncludeBase<InventoryImportModel, InventoryDocument>()
                .ForMember(dest => dest.ProductSupplyings, opt => opt.MapFrom(src => src.ProductItems))
                .ForMember(dest => dest.EquipmentSupplyings, opt => opt.MapFrom(src => src.EquipmentItems));

            CreateMap<ProductImportItem, ProductSupplying>();
            CreateMap<EquipmentImportItem, EquipmentSupplying>();

            CreateMap<InventoryDocument, InventoryImportDetailProjection>();

            CreateMap<ProductSupplying, ProductSupplyingProjection>();
            CreateMap<EquipmentSupplying, EquipmentSupplyingProjection>();

            CreateMap<InventoryDocument, UpdateInventoryImportResponse>()
                .IncludeBase<InventoryDocument, InventoryImportDetailProjection>()
                .ForMember(dest => dest.ProductSupplyings, opt => opt.MapFrom(src => src.ProductSupplyings))
                .ForMember(dest => dest.EquipmentSupplyings, opt => opt.MapFrom(src => src.EquipmentSupplyings));
        }
    }
}
