using Application.Feature.Common.Validators.Categories;
using FluentValidation;

namespace Application.Feature.Categories.Command.Create;

public class CreateCategoryValidation : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidation()
    {
        Include(new CategoryModelValidation());
    }
}
