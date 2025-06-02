using Application.Feature.Common.Projections.Services;
using Mediator;

namespace Application.Feature.Categories.Command.Create;

public class CreateCategoryCommand : CategoryModel, IRequest
{
    public string? Id { get; set; }
}
