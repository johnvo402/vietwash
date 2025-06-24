using Application.Feature.Services.Queries.Detail;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Mapping.Users;

public static class UserMapping
{
    public static UserDTO UserDTOResponse(this User user) =>
        new()
        {
            Id = user.Id,
            PublicId = user.PublicId,
            DisplayName = user.DisplayName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
}
