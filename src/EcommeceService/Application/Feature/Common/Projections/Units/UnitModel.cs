using Domain.Aggregates.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitModel
    {
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } = ActivationStatus.active;
    }
}
