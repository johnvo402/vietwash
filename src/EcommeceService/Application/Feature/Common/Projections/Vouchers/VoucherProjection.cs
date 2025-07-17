using Application.Common.Security;
using Contracts.Application.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Vouchers
{
    public class VoucherProjection : BaseResponse
    {
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        [File]
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
        public virtual void MappingFrom(Voucher voucher)
        {
            Id = voucher.Id;
            Code = voucher.Code;
            Title = voucher.Title;
            ImgUrl = voucher.ImgUrl;
            Barcode = voucher.Barcode;
            DiscountFixed = voucher.DiscountFixed;
            DiscountValue = voucher.DiscountValue;
            TotalQuantity = voucher.TotalQuantity;
            UsedQuantity = voucher.UsedQuantity;
            CustomerGroups = CustomerGroups;
            StartAt = voucher.StartAt;
            EndAt = voucher.EndAt;
            Status = voucher.Status;
            Description = voucher.Description;
            PublicId = voucher.PublicId;
            CreatedAt = voucher.CreatedAt;
            CreatedBy = voucher.CreatedBy;
            UpdatedAt = voucher.UpdatedAt;
            UpdatedBy = voucher.UpdatedBy;

        }
    }
}
