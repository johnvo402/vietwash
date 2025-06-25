using Application.Features.Common.Mapping;
using Application.Features.Common.Projections.FundBehaviors;
using Application.Features.Common.Projections.Users;
using Contracts.Application.Common;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.Funds
{
    public class FundProjection : BaseResponse
    {
        public string? Code { get; private set; }
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public long FundBehaviorId { get; set; }
        public string? Note { get; set; }
        public FundStatus Status { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public FundBehaviorProjection? FundBehavior { get; set; }
        public DateTimeOffset? TransactionDate { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public FundType? Type { get; set; }
        public long? ReferenceId { get; set; }
        public object? Metadata { get; set; }
        public UserDTO? User { get; set; }

        public virtual void MappingFrom(Fund fund)
        {
            Id = fund.Id;
            PublicId = fund.PublicId;
            CreatedAt = fund.CreatedAt;
            CreatedBy = fund.CreatedBy;
            UpdatedAt = fund.UpdatedAt;
            UpdatedBy = fund.UpdatedBy;

            Code = fund.Code;
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
            Metadata = fund.Metadata;

            if (fund.User != null)
            {
                User = fund.User.ToUserDTOResponse();
            }
            if (fund.FundBehavior != null)
            {
                FundBehavior = fund.FundBehavior.ToFundBehaviorProjection();
            }
        }
    }
}
