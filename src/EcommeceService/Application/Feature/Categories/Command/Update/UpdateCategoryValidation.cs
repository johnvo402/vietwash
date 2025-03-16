using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Categories;
using FluentValidation;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryValidation : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidation()
    {
        Include(new UpdateCategoryCommandValidation());
    }
}
