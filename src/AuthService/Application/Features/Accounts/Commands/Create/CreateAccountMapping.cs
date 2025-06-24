using Application.Features.Common.Projections.Accounts;
using Contracts.Extensions;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Create;

public static class CreateAccountMapping
{
    public static Account ToAccount(this CreateAccountCommand command, string code)
    {
        return new Account(
            displayName: command.DisplayName?.Trim(),
            password: HashPassword(command.Password!),
            email: command.Email,
            phoneNumber: command.PhoneNumber!,
            role: command.Role!,
            code: code
        )
        {
            Gender = command.Gender,
            Status = command.Status,
            BirthDay = DateOnly.FromDateTime((DateTime)command.BirthDay!),
            BranchAccounts = command.BranchAccounts.ToListMapping(x => new BranchAccount
            {
                BranchId = x.BranchId,
                BranchName = x.BranchName,
            }),
        };
    }

    public static CreateAccountResponse ToCreateAccountResponse(this Account user)
    {
        var response = new CreateAccountResponse();
        response.MappingFrom(user);
        return response;
    }
}
