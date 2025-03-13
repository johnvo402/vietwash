using Application.Feature.Common.Projections.Units;
using JohnChum.SharedKernel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceDetailProjection : ServiceProjection
    {
        public List<UnitRelationProjection> UnitRelation { get; set; } = [];
    }
}
