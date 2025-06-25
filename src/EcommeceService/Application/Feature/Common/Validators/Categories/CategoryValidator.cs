using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Categories.Command.Update;
using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Services;
using FluentValidation;
using Contracts.Common.Messages;

namespace Application.Feature.Common.Validators.Categories;

public class CategoryValidator : AbstractValidator<CategoryModel>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IActionAccessorService accessorService;

    public CategoryValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
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
                    .Create<Category>()
                    .Property(x => x.Name)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messager
                    .Create<Category>()
                    .Property(x => x.Name)
                    .Message(MessageType.MaximumLength)
                    .Negative()
                    .Build()
            );
    }
}
