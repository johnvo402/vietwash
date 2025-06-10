using Application.Feature.Orders.Command.UpdateStatus;
using Contracts.Routers;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Command.UpdateStautus
{
    public class UpdateStatusInventoryImportCommand : IRequest<UpdateStatusInventoryImportResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long InventoryImportId { get; set; } = default!;
        public InventoryDocumentStatus? Status { get; set; }

    }
}
