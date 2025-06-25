using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Common.Projections.Warehouses;
using Domain.Aggregates.Warehouses;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Application.Features.Warehouses.Commands.Update
{
    public static class UpdateWarehouseMapping
    {
        public static void UpdateWarehouse(this Warehouse warehouses, WarehouseModel model)
        {
            warehouses.Update(
                name: model.Name,
                code: model.Code,
                description: model.Description,
                status: model.Status
            );
        }
    }
}
