using Application.Feature.Common.Projections.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderResponse : OrderProjection
    {
        public string Message { get; set; } = default!;
    }
}
