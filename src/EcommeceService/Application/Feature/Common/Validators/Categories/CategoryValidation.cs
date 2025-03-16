using Application.Feature.Categories.Command.Update;
using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Services;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Feature.Common.Validators.Categories;

public class CategoryModelValidation : AbstractValidator<CategoryModel>
{
    public CategoryModelValidation()
    {
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

public class UpdateCategoryCommandValidation : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidation()
    {
        ApplyRules();
    }

    private void ApplyRules()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithState(x =>
                Messager
                    .Create<UpdateCategoryCommand>()
                    .Property(x => x.CategoryId)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x => Ulid.TryParse(x, out _))
            .WithState(x =>
                Messager
                    .Create<UpdateCategoryCommand>()
                    .Property(x => x.CategoryId)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );
    }
}
