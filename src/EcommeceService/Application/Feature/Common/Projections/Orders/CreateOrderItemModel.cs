using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
	public class CreateOrderItemModel
	{
		public string ServiceId { get; set; }
		public string UnitRelationId { get; set; }
		public decimal Price { get; set; }
	}
}
