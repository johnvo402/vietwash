using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Category? getCategory = await unitOfWork
            .Repository<Category>()
            .FindByIdAsync(command.CategoryId);

        if (getCategory == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "Category not found",
                    Messager.Create<Category>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        command.MapUpdateToEntity(getCategory);

        getCategory.Path = await GenerateCategoryPathAsync(
            getCategory.Id,
            command.Category.ParentId,
            cancellationToken
        );

        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Category>().UpdateAsync(getCategory);

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GenerateCategoryPathAsync(
        string id,
        string? parentId,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(parentId))
        {
            return id.ToLower();
        }

        var parent = await unitOfWork
            .Repository<Category>()
            .FindByIdAsync(parentId, cancellationToken);

        if (parent == null)
            return id.ToLower();

        return $"{parent.Path}.{id.ToLower()}";
    }
}
