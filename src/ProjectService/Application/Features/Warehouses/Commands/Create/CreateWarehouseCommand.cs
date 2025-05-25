using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Application.Features.Common.Projections.Warehouses;
using Domain.Aggregates.Warehouses;
using Mediator;

namespace Application.Features.Warehouses.Commands.Create
{
    public class CreateWarehouseCommand : WarehouseModel, IRequest<CreateWarehouseResponse>
    {
    }
}
