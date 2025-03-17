using Mediator;

namespace Application.Feature.Categories.Command.Delete;

public record DeleteCategoryCommand(Ulid CategoryId) : IRequest;
