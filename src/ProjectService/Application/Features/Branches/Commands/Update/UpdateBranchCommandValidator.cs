using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using FluentValidation;

namespace Application.Features.Branches.Commands.Update
{
    public partial class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
    {
        public UpdateBranchCommandValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService actionAccessorService
        ) { }
    }
}
