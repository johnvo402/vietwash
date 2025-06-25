using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Validators.Warehouses;
using FluentValidation;

namespace Application.Features.Warehouses.Commands.Update
{
    public partial class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
    {
        public UpdateWarehouseCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService actionAccessorService)
        {
        }
    }
}
