using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Users.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Vouchers
{
    public class Voucher : AggregateRoot
    {
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? ImgUrl { get; set; }
        public string Barcode { get; set; } = default!;
        public bool DiscountFixed { get; set; }
        public decimal DiscountValue { get; set; }
        public int TotalQuantity { get; set; }
        public int UsedQuantity { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public ActivationStatus Status { get; set; }

        public ICollection<VoucherCustomer> VoucherCustomers { get; set; } =
            new List<VoucherCustomer>();
        public ICollection<VoucherCustomerGroup> VoucherCustomerGroups { get; set; } =
            new List<VoucherCustomerGroup>();

        public Voucher() { }

        public Voucher(
            string code,
            string title,
            string? imgUrl,
            string barcode,
            bool discountFixed,
            decimal discountValue,
            int totalQuantity,
            int usedQuantity,
            DateTimeOffset? startAt,
            DateTimeOffset? endAt,
            ActivationStatus status,
            string? description = null
        )
        {
            Code = Guard.Against.NullOrWhiteSpace(code);
            Title = Guard.Against.NullOrWhiteSpace(title);
            ImgUrl = imgUrl;
            Barcode = Guard.Against.NullOrWhiteSpace(barcode);
            DiscountFixed = discountFixed;
            DiscountValue = Guard.Against.NegativeOrZero(discountValue);
            TotalQuantity = Guard.Against.NegativeOrZero(totalQuantity);
            UsedQuantity = Guard.Against.Negative(usedQuantity);
            StartAt = startAt;
            EndAt = endAt;
            Status = Guard.Against.EnumOutOfRange(status);
            Description = description?.Trim();
        }

        public void Update(
            string? code = null,
            string? title = null,
            string? imgUrl = null,
            string? barcode = null,
            bool? discountFixed = null,
            decimal? discountValue = null,
            int? totalQuantity = null,
            int? usedQuantity = null,
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
                DiscountValue = Guard.Against.NegativeOrZero(discountValue.Value);

            if (totalQuantity.HasValue)
                TotalQuantity = Guard.Against.NegativeOrZero(totalQuantity.Value);

            if (usedQuantity.HasValue)
                UsedQuantity = Guard.Against.NegativeOrZero(usedQuantity.Value);

            if (startAt.HasValue)
                StartAt = startAt;

            if (endAt.HasValue)
                EndAt = endAt;

            if (status.HasValue)
                Status = Guard.Against.EnumOutOfRange(status.Value);

            if (description != null)
                Description = description.Trim();
        }

        public void AssignToCustomerGroup(CustomerGroup group)
        {
            if (!VoucherCustomerGroups.Any(x => x.Group == group))
            {
                VoucherCustomerGroups.Add(
                    new VoucherCustomerGroup { Voucher = this, Group = group }
                );
            }
        }

        public void AssignToCustomer(long customerId)
        {
            if (!VoucherCustomers.Any(x => x.CustomerId == customerId))
            {
                VoucherCustomers.Add(
                    new VoucherCustomer { Voucher = this, CustomerId = customerId }
                );
            }
        }

        public void UpdateCustomerGroups(IEnumerable<CustomerGroup> newGroups)
        {
            VoucherCustomerGroups.Clear();
            foreach (var group in newGroups.Distinct())
            {
                AssignToCustomerGroup(group);
            }
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            return false;
        }
    }
}
