using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
	public class OrderItemModel
	{
		public Ulid ServiceId { get; set; }
		public Ulid UnitRelationId { get; set; }
		public decimal Price { get; set; }
	}
}
