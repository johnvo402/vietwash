using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediator;

namespace Application.Features.Warehouses.Commands.Delete
{
    public record class DeleteWarehouseCommand(long id) : IRequest
    {
    }
}
