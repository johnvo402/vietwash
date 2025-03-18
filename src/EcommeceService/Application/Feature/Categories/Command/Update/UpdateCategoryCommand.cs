using Application.Feature.Common.Projections.Services;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryCommand : IRequest
{
    [FromRoute(Name = RouterBase.Id)]
    public string CategoryId { get; set; } = string.Empty;

    [FromBody]
    public CategoryModel Category { get; set; } = default!;
}
