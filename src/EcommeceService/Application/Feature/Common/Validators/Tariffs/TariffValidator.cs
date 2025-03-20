using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Tariffs;
using FluentValidation;
using Domain.Aggregates.Tariffs;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Application.Feature.Common.Validators.Tariffs
{
    public class TariffValidator : AbstractValidator<TariffModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;
        public TariffValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }
        private void ApplyRules()
        {
            RuleFor(t => t.Name)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Tariff>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                    .Create<Tariff>()
                    .Property(x => x.Name)
                    .Message(MessageType.MaximumLength)
                    .Build());
            RuleFor(t => t.Disable)
                .NotNull()
                .WithState(x =>
                    Messager
                        .Create<Tariff>()
                        .Property(x => x.Disable)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
        );
        }
    }
}