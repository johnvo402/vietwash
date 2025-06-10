using Application.Feature.Common.Projections.InventoryImports;
using AutoMapper;
using Domain.Aggregates.Inventories;

namespace Application.Feature.InventoryImports.Command.Create
{
    public class CreateInventoryImportMapping : Profile
    {
        public CreateInventoryImportMapping()
        {
            CreateMap<InventoryImportModel, InventoryDocument>();

            CreateMap<CreateInventoryImportCommand, InventoryDocument>()
                .IncludeBase<InventoryImportModel, InventoryDocument>()
                .ForMember(dest => dest.ProductSupplyings, opt => opt.MapFrom(src => src.ProductItems))
                .ForMember(dest => dest.EquipmentSupplyings, opt => opt.MapFrom(src => src.EquipmentItems));

            CreateMap<ProductImportItem, ProductSupplying>();
            CreateMap<EquipmentImportItem, EquipmentSupplying>();

        }
    }
}
