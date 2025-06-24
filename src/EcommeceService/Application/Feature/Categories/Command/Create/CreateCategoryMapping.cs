using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Command.Create;

public static class CreateCategoryMapping
{
    public static Category ToEntity(this CreateCategoryCommand command)
    {
        return new Category(
            name: command.Name!,
            parentId: command.ParentId,
            status: command.Status
        );
    }
}
