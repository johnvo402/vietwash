using System.Linq.Expressions;
using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Queries.List;

public class ListAccountMapping
{
    public static Expression<Func<Account, ListAccountResponse>> Selector()
    {
        return account => new ListAccountResponse
        {
            Id = account.Id,
            PublicId = account.PublicId,
            CreatedAt = account.CreatedAt,
            CreatedBy = account.CreatedBy,
            UpdatedAt = account.UpdatedAt,
            UpdatedBy = account.UpdatedBy,

            DisplayName = account.DisplayName,
            Email = account.Email,
            PhoneNumber = account.PhoneNumber,
            AvtUrl = account.AvtUrl,
            Role = account.Role,

            Status = account.Status,
        };
    }
}
