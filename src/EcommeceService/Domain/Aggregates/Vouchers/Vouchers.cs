using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Equipments.Enums;
using Shared.Kernel.Common;
using Mediator;
using Domain.Aggregates.Enums;

namespace Domain.Aggregates.Vouchers
{
    public class Voucher : AggregateRoot
    {
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string ImgUrl { get; set; } = default!;
        public string Barcode { get; set; } = default!;
        public bool DiscountFixed { get; set; } = default!;
        public decimal DiscountValue { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public DateTimeOffset StartAt { get; set; } = default!;
        public DateTimeOffset EndAt { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;

        public ICollection<VoucherCustomer> VoucherCustomers { get; set; } =
            new List<VoucherCustomer>();

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
