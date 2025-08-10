using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using FluentValidation;

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

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithState(x =>
                    Messager
                        .Create<Service>()
                        .Property(s => s.Status)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithState(x =>
                    Messager
                        .Create<Service>()
                        .Property(s => s.Description)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.UnitRelations)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<ServiceResourceModel>(nameof(ServiceResource))
                        .Property(s => s.ProductId)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                );
            RuleForEach(x => x.UnitRelations)
                .ChildRules(x =>
                {
                    x.RuleForEach(x => x.ServiceResources)
                        .SetValidator(x => new ServiceResourceValidator());
                });
        }
    }

    public class ServiceResourceValidator : AbstractValidator<ServiceResourceModel>
    {
        public ServiceResourceValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<ServiceResourceModel>(nameof(ServiceResource))
                        .Property(s => s.ProductId)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                );
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<ServiceResourceModel>(nameof(ServiceResource))
                        .Property(s => s.Quantity)
                        .Message(MessageType.Empty)
                        .Negative()
                        .Build()
                )
                .GreaterThan(0)
                .WithState(x =>
                    Messager
                        .Create<ServiceResourceModel>(nameof(ServiceResource))
                        .Property(s => s.Quantity)
                        .Message(MessageType.GreaterThan)
                        .Negative()
                        .Build()
                );
        }
    }
}
