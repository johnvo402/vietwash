using Application.Feature.Common.Projections.InventoryImports;
using Application.Feature.Common.Projections.Services;
using Application.Feature.Services.Command.Update;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Command.Update
{
    public class UpdateInventoryImportCommand : IRequest<UpdateInventoryImportResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long InventoryImportId { get; set; } = default!;
        [FromBody]
        public InventoryImportUpdateModel Body { get; set; } = default!;
    }
}
