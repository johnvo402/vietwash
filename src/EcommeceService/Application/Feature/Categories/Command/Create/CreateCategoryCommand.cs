using Application.Feature.Common.Projections.Services;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Categories.Command.Create;

public class CreateCategoryCommand : CategoryModel, IRequest<Result>;
