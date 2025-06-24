using Application.Features.Common.Projections.Users;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Update;

public static class UpdateUserMapping
{
    public static User FromUpdateUser(this User user, UserModel update)
    {
        user.Update(
            update.DisplayName,
            update.Email,
            update.PhoneNumber,
            update.BirthDay != null ? DateOnly.FromDateTime((DateTime)update.BirthDay) : null,
            update.Status,
            update.Role,
            update.AvtUrl
        );
        return user;
    }
}
