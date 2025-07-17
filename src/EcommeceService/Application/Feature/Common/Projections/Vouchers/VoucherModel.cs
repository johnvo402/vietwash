using Domain.Aggregates.Enums;
using Domain.Aggregates.Users.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Vouchers
{
    public class VoucherModel
    {
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string ImgUrl { get; set; } = default!;
        public string Barcode { get; set; } = default!;
        public bool DiscountFixed { get; set; } = default!;
        public decimal DiscountValue { get; set; } = default!;
        public int TotalQuantity { get; set; } = default!;
        public int UsedQuantity { get; set; } = default!;
        public List<CustomerGroup> CustomerGroups { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public DateTimeOffset StartAt { get; set; } = default!;
        public DateTimeOffset EndAt { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;

    }
}
