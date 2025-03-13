using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitRelationProjection
    {
        public Ulid ServiceId { get; set; }
        public Ulid UnitId { get; set; }
        public bool BaseUnit { get; set; }
        public decimal Price { get; set; }
    }
}
