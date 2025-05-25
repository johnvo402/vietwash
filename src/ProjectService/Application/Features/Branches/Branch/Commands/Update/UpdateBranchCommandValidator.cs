using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Branches;
using FluentValidation;

namespace Application.Features.Branches.Branch.Commands.Update
{
    public partial class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
    {
        public UpdateBranchCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService actionAccessorService)
        {
            
        }
    }
}
