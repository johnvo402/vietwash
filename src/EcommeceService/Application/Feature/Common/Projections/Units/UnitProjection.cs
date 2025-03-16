using JohnChum.SharedKernel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Units
{
	public class UnitProjection : BaseEntity
	{
        public string Name { get; set; } = default!;

	}
}
