

using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Services;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Feature.Common.Validators.Services
{
    public class ServiceValidator : AbstractValidator<ServiceModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public ServiceValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
        {
            this.unitOfWork = unitOfWork;
            this.accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithState(x =>
                Messager
                    .Create<Service>()
                    .Property(x => x.Name)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messager
                    .Create<Service>()
                    .Property(x => x.Name)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

        }
    }
}
