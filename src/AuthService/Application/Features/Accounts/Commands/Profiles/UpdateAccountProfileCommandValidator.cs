using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Accounts;
using FluentValidation;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommandValidator : AbstractValidator<UpdateAccountProfileCommand>
{
    public UpdateAccountProfileCommandValidator(
        IUnitOfWork unitOfWork,
        IActionAccessorService accessorService
    )
    {
        Include(new AccountValidator(unitOfWork, accessorService));
    }
}
