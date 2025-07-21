using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Equipments;
using Application.Feature.Common.Projections.Orders;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Products;
using FluentValidation;

namespace Application.Feature.Common.Validators.Equipments
{
    public class EquipmentValidator : AbstractValidator<EquipmentModel>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public EquipmentValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
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
                        .Create<Equipment>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<EquipmentModel>(nameof(Equipment))
                        .Property(x => x.Name)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<EquipmentModel>(nameof(Equipment))
                        .Property(x => x.Description)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Equipment>()
                        .Property(x => x.Code)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MustAsync(IsCodeExistsAsync)
                .WithState(_ =>
                    Messager
                        .Create<Equipment>()
                        .Property(x => x.Code)
                        .Message(MessageType.Found)
                        .Negative()
                        .Build()
                );
        }

        private async Task<bool> IsCodeExistsAsync(string code, CancellationToken cancellation)
        {
            return !await unitOfWork
                .Repository<Equipment>()
                .AnyAsync(p => p.Code == code, cancellation);
        }
    }

    public class EquipmentUpdateValidator : AbstractValidator<EquipmentUpdateModel>
    {
        public EquipmentUpdateValidator()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithState(x =>
                    Messager
                        .Create<Equipment>()
                        .Property(x => x.Name)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                )
                .MaximumLength(256)
                .WithState(x =>
                    Messager
                        .Create<EquipmentModel>(nameof(Equipment))
                        .Property(x => x.Name)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithState(x =>
                    Messager
                        .Create<EquipmentModel>(nameof(Equipment))
                        .Property(x => x.Description)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );
        }
    }
}
