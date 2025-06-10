using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
    public class UpdateOrderItemModel
    {
        public long OrderItemId { get; set; }
        public long ServiceId { get; set; }
        public long UnitRelationId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
