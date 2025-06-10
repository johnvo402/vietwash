using Domain.Aggregates.Orders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
    public class UpdateOrderModel
    {
        public string? Note { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public bool? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? PaymentAmount { get; set; } // Số tiền thanh toán bổ sung
        public List<UpdateOrderItemModel>? OrderItems { get; set; } // Có thể cập nhật hoặc không
    }
}
