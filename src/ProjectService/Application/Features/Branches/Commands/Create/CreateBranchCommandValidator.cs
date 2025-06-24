using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Branches;
using FluentValidation;

namespace Application.Features.Branches.Commands.Create
{
    public partial class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
    {
        public CreateBranchCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService actionAccessorService)
        {
            Include( new BranchValidator(unitOfWork, actionAccessorService) );
        }

    }
}
