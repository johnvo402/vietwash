using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
    public class CreateOrderItemModel
    {
        public long ServiceId { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
