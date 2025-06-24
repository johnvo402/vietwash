using Application.Feature.Common.Projections.Services;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryCommand : IRequest<Result>
{
    [FromRoute(Name = RouterBase.Id)]
    public string CategoryId { get; set; } = string.Empty;

    [FromBody]
    public CategoryModel Category { get; set; } = default!;
}
