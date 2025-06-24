using System.Linq.Expressions;
using Domain.Aggregates.Funds;

namespace Application.Features.Funds.Queries.List;

public static class ListFundMapping
{
    public static Expression<Func<Fund, ListFundResponse>> Selector()
    {
        return fund => new ListFundResponse
        {
            Id = fund.Id,
            PublicId = fund.PublicId,
            CreatedAt = fund.CreatedAt,
            CreatedBy = fund.CreatedBy,
            UpdatedAt = fund.UpdatedAt,
            UpdatedBy = fund.UpdatedBy,
            Name = fund.Name,
            Type = fund.Type,
            FundBehaviorId = fund.FundBehaviorId,
            Amount = fund.Amount,
            Note = fund.Note,
            TransactionDate = fund.TransactionDate,
            PaymentMethod = fund.PaymentMethod,
            ReferenceId = fund.ReferenceId,
        };
    }
}
