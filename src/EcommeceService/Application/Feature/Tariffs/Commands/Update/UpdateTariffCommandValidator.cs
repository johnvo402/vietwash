using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Tariffs;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffCommandValidator : AbstractValidator<UpdateTariffCommand>
    {
        public UpdateTariffCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            RuleFor(t => t.Tariff)
                .SetValidator(new TariffValidator(unitOfWork, accessorService));
            RuleFor(x => x.Tariff)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateTariffCommand>()
                    .Property(x => x.Tariff!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
        }
    }
}