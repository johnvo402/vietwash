using Application.Features.Common.Projections.Users;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Users.Commands.Update;

public static class UserMappingExtensions
{
    public static User FromUpdateModel(this User entity, UpdateAccount model)
    {
        entity.Update(
            displayName: model.DisplayName,
            email: model.Email,
            phoneNumber: model.PhoneNumber,
            birthDay: model.BirthDay,
            gender: model.Gender,
            role: model.Role,
            status: model.Status
        );
        if (!string.IsNullOrWhiteSpace(model.AvtUrl))
            entity.AvtUrl = model.AvtUrl;
        if (model.CustomerGroup.HasValue)
            entity.CustomerGroup = model.CustomerGroup;

        return entity;
    }

    public static UpdateUserResponse ToUpdateUserResponse(this User user)
    {
        var response = new UpdateUserResponse();
        response.MappingFrom(user);
        return response;
    }
}
