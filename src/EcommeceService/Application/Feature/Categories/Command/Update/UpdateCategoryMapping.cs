using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Command.Update;

public static class UpdateCategoryMapper
{
    public static void MapUpdateToEntity(this UpdateCategoryCommand command, Category category)
    {
        category.Update(
            name: command.Category.Name,
            parentId: command.Category.ParentId,
            status: command.Category.Status
        );
    }
}
