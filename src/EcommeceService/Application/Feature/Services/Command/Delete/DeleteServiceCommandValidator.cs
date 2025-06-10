using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Services.Command.Update;
using Domain.Aggregates.Services;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Services.Command.Delete
{
    public class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public DeleteServiceCommandValidator(
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
            RuleFor(x => x.ServiceId)
                .NotEmpty()
                .WithState(x =>
                        Messager
                            .Create<Service>()
                            .Property(x => x.Id)
                            .Message(MessageType.Null)
                            .Negative()
                            .Build()
                );

        }
    }
}
