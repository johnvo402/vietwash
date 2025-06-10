using Application.Feature.Common.Projections.Units;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Units.Command.Create
{
    public class CreateUnitCommand : UnitModel, IRequest<CreateUnitResponse>;
}
