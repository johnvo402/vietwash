using Contracts.Application.Common;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using JohnChum.SharedKernel.Application.Common;

namespace Application.Features.Common.Projections.Funds
{
    public class FundProjection : BaseResponse
    {
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public long FundBehaviorId { get; set; }
        public string? Note { get; set; }
        public FundStatus Status { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public DateTimeOffset? TransactionDate { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public FundType? Type { get; set; }
        public long? ReferenceId { get; set; }

        public virtual void MappingFrom(Fund fund)
        {
            Id = fund.Id;
            PublicId = fund.PublicId;
            CreatedAt = fund.CreatedAt;
            CreatedBy = fund.CreatedBy;
            UpdatedAt = fund.UpdatedAt;
            UpdatedBy = fund.UpdatedBy;

            Name = fund.Name;
            Type = fund.Type;
            Amount = fund.Amount;
            FundBehaviorId = fund.FundBehaviorId;
            Note = fund.Note;
            Status = fund.Status;
            PaymentMethod = fund.PaymentMethod;
            TransactionDate = fund.TransactionDate;
            BranchId = fund.BranchId;
            Type = fund.Type;
            FundBehaviorId = fund.FundBehaviorId;
            ReferenceId = fund.ReferenceId;
        }
    }
}
