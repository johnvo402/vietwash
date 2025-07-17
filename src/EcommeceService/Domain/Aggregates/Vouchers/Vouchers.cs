using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Equipments.Enums;
using Shared.Kernel.Common;
using Mediator;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users.Enums;
using Ardalis.GuardClauses;

namespace Domain.Aggregates.Vouchers
{
    public class Voucher : AggregateRoot
    {
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? ImgUrl { get; set; }
        public string Barcode { get; set; } = default!;
        public bool DiscountFixed { get; set; } = default!;
        public decimal DiscountValue { get; set; } = default!;
        public int TotalQuantity { get; set; } = default!;
        public int UsedQuantity { get; set; } = default!;
        public List<CustomerGroup> CustomerGroups { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public ActivationStatus Status { get; set; } = default!;

        public ICollection<VoucherCustomer> VoucherCustomers { get; set; } =
            new List<VoucherCustomer>();

        public Voucher()
        {
            CustomerGroups = new List<CustomerGroup>();
            VoucherCustomers = new List<VoucherCustomer>();
        }
        public Voucher(
   string code,
   string title,
   string? imgUrl,
   string barcode,
   bool discountFixed,
   decimal discountValue,
   int totalQuantity,
   int usedQuantity,
   List<CustomerGroup> customerGroups,
   DateTimeOffset? startAt,
   DateTimeOffset? endAt,
   ActivationStatus status,
   string? description = null
)
        {
            Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
            Title = Guard.Against.NullOrWhiteSpace(title, nameof(title));
            ImgUrl = imgUrl;
            Barcode = Guard.Against.NullOrWhiteSpace(barcode, nameof(barcode));
            DiscountFixed = discountFixed;
            DiscountValue = Guard.Against.NegativeOrZero(discountValue, nameof(discountValue));
            TotalQuantity = Guard.Against.NegativeOrZero(totalQuantity, nameof(totalQuantity));
            UsedQuantity = Guard.Against.Negative(usedQuantity, nameof(usedQuantity));
            CustomerGroups = customerGroups ?? new List<CustomerGroup>();
            StartAt = startAt;
            EndAt = endAt;
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));
            Description = description?.Trim();
        }
        public void Update(
            string? code = null,
            string? title = null,
            string? imgUrl = null,
            string? barcode = null,
            bool? discountFixed = null,
            decimal? discountValue = null,
            int? totalQuantity = null, int? usedQuantity = null,
            List<CustomerGroup>? customerGroups = null,
            DateTimeOffset? startAt = null,
            DateTimeOffset? endAt = null,
            ActivationStatus? status = null,
            string? description = null
        )
        {
            if (!string.IsNullOrWhiteSpace(code))
                Code = code.Trim();
            if (!string.IsNullOrWhiteSpace(title))
                Title = title.Trim();
            if (!string.IsNullOrWhiteSpace(imgUrl))
                ImgUrl = imgUrl.Trim();
            if (!string.IsNullOrWhiteSpace(barcode))
                Barcode = barcode.Trim();
            if (discountFixed.HasValue)
                DiscountFixed = discountFixed.Value;
            if (discountValue.HasValue)
                DiscountValue = Guard.Against.NegativeOrZero(discountValue.Value, nameof(discountValue));
            if (totalQuantity.HasValue)
                TotalQuantity = Guard.Against.NegativeOrZero(totalQuantity.Value, nameof(totalQuantity));
            if (usedQuantity.HasValue)
                UsedQuantity = Guard.Against.NegativeOrZero(usedQuantity.Value, nameof(usedQuantity));
            if (customerGroups != null)
                CustomerGroups = customerGroups;
            if (startAt.HasValue)
                StartAt = startAt;
            if (endAt.HasValue)
                EndAt = endAt;
            if (status.HasValue)
                Status = Guard.Against.EnumOutOfRange(status.Value, nameof(status));
            if (description != null)
                Description = description.Trim();
        }
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
