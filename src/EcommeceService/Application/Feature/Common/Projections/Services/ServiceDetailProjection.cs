using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Categories;
using Application.Feature.Common.Projections.Units;
using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceDetailProjection : ServiceProjection
    {
        public CategoryProjection Category { get; set; } = default!;
        public string? Description { get; set; }
        public long BranchId { get; set; } = default!;
        public List<UnitRelationProjection> UnitRelations { get; set; } = [];
    }
}
