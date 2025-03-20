using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Delete
{
public record DeleteTariffCommand(Ulid TariffId) : IRequest
    {
        
    }
}