using Contracts.Utils;
using Domain.Aggregates.Services;

namespace Application.Feature.Categories.Command.Create;

public static class CreateCategoryMapping
{
    public static Category ToEntity(this CreateCategoryCommand command)
    {
        string code = Generator.GenerateCode("DM", 6);
        var entity = new Category(
            name: command.Name!,
            parentId: command.ParentId,
            status: command.Status,
            code: code
        );

        return entity;
    }
}
