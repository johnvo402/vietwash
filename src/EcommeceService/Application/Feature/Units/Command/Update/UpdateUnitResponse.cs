using Application.Feature.Common.Projections.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Units.Command.Update
{
    public class UpdateUnitResponse : UnitProjection
    {
        public string Message { get; set; } = default!;
    }
}
