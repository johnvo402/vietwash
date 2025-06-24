using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Create;

public static class CreateUserMapping
{
    public static User ToUser(this CreateAccountEvent command)
    {
        return new User(
            displayName: command.DisplayName!.Trim(),
            email: command.Email!,
            phoneNumber: command.PhoneNumber!,
            role: command.Role!,
            code: command.Code!
        )
        {
            Id = command.Id,
            PublicId = command.PublicId,
            Gender = command.Gender,
            Status = command.Status,
            BirthDay = command.BirthDay,
            AvtUrl = command.AvtUrl,
            Disabled = command.Disabled,
            CustomerGroup = command.CustomerGroup,
            // BranchUsers = command.BranchUsers.ToListMapping(x => new BranchUser
            // {
            //     BranchId = x.BranchId,
            //     BranchName = x.BranchName,
            // }),
        };
    }
}
