using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Feature.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Orders
{
    public class CreateOrderModel
    {
        public string? CustomerId { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? ReceivedTime { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public bool? DiscountType { get; set; } // true = percentage, false = fixed amount
        public decimal? DiscountValue { get; set; }
        public List<CreateOrderItemModel> OrderItems { get; set; } = [];
        public decimal PaymentAmount { get; set; } // Số tiền thanh toán ban đầu
    }
}
