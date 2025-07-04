using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommandValidator : AbstractValidator<UpdateAccountProfileCommand>
{
    public UpdateAccountProfileCommandValidator(IActionAccessorService accessorService)
    {
        Include(new AccountValidator(accessorService));
    }
}
