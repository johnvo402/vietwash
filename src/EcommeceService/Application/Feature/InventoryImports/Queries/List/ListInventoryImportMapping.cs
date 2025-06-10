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

namespace Application.Feature.InventoryImports.Queries.List
{
    public class ListInventoryImportMapping : Profile
    {
        public ListInventoryImportMapping()
        {
            CreateMap<InventoryDocument, InventoryImportProjection>();

            CreateMap<InventoryDocument, ListInventoryImportResponse>()
                .IncludeBase<InventoryDocument, InventoryImportProjection>();


        }
    }
}
