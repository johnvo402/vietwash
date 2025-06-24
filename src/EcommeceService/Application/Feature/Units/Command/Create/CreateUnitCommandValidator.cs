using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Units;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Units.Command.Create
{
    public partial class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IActionAccessorService accessorService;

        public CreateUnitCommandValidator(
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
            // Tái sử dụng các quy tắc từ UnitValidator
            Include(new UnitValidator(unitOfWork, accessorService));

            // Kiểm tra tính duy nhất của Name
            RuleFor(x => x.Name)
                .MustAsync((name, cancellationToken) => IsNameUniqueAsync(name!, cancellationToken))
                .WithState(x =>
                    Messager
                        .Create<Unit>()
                        .Property(x => x.Name)
                        .Message(MessageType.Existence)
                        .Build()
                );
        }

        // Phương thức kiểm tra tính duy nhất của Name
        private async Task<bool> IsNameUniqueAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            return !await unitOfWork
                .Repository<Unit>()
                .AnyAsync(x => EF.Functions.ILike(x.Name, name), cancellationToken);
        }
    }
}
