using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Categories;
using Domain.Aggregates.Services;
using FluentValidation;
using Contracts.Common.Messages;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public UpdateCategoryCommandValidator(
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
        RuleFor(x => x.Category.Name)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateCategoryCommand>()
                    .Property(x => x.Category.Name)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messager
                    .Create<UpdateCategoryCommand>()
                    .Property(x => x.Category.Name)
                    .Message(MessageType.MaximumLength)
                    .Negative()
                    .Build()
            );
    }
}
