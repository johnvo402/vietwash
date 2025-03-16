using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Tariffs;
using Domain.Aggregates.Tariffs;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Wangkanai.Extensions;

namespace Application.Feature.Tariffs.Commands.Create
{
    public partial class CreateTariffCommandValidator : AbstractValidator<CreateTariffCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;


        public CreateTariffCommandValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService accessorService
        )
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }
        private void ApplyRules()
        {
            RuleFor(t => t.Payload)
                .SetValidator(new TariffValidator(unitOfWork, accessorService));
            RuleFor(t => t.Payload!.Disable)
                .Equal(false)
                .WithState(x =>
                    Messager
                        .Create<Tariff>()
                        .Property(x => x.Name)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build())
                .WithMessage("New tariff must be enabled when created.");
        }
    }
}