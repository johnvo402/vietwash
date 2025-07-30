using System.Linq.Expressions;
using Contracts.Extensions;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Update;

public static class UpdateAccountMapping
{
    public static Account FromUpdateUser(this Account user, UpdateAccount update)
    {
        user.Update(
            update.DisplayName,
            update.Email,
            update.PhoneNumber,
            update.BirthDay != null ? DateOnly.FromDateTime((DateTime)update.BirthDay) : null,
            status: update.Status,
            role: update.Role,
            gender: update.Gender
        );
        user.AvtUrl = update.AvtUrl;
        user.BranchAccounts?.Clear();

        if (update.BranchAccounts != null)
        {
            foreach (var x in update.BranchAccounts)
            {
                user.BranchAccounts?.Add(
                    new BranchAccount { BranchId = x.BranchId, BranchName = x.BranchName }
                );
            }
        }
        return user;
    }
}
