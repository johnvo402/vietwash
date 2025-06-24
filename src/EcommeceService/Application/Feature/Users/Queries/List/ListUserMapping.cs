using System.Linq.Expressions;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Queries.List;

public static class ListUserMapping
{
    public static Expression<Func<User, ListUserResponse>> Selector() =>
        user => new ListUserResponse
        {
            Id = user.Id,
            PublicId = user.PublicId,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,

            DisplayName = user.DisplayName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            BirthDay = user.BirthDay,
            Gender = user.Gender,
            Avatar = user.AvtUrl,
            CustomerGroup = user.CustomerGroup,
            Status = user.Status,
            Role = user.Role,
        };
}
