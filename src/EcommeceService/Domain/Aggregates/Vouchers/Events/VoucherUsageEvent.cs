using Domain.Aggregates.Products;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Vouchers.Events
{
    public class VoucherUsageEvent : INotification
    {
        public long VoucherId { get; init; }
        public long CustomerId { get; init; }
        public long BranchId { get; init; }
        public long OrderId { get; init; }
        public decimal DiscountApply { get; init; }
    }
}

