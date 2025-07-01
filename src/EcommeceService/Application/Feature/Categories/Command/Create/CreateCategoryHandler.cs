using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Categories.Command.Create;

public class CreateCategoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Category mappingCategory = command.ToEntity();
        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            mappingCategory.Path = await GenerateCategoryPathAsync(
                mappingCategory.Code,
                command.ParentId,
                cancellationToken
            );
            //add into database
            Category category = await unitOfWork
                .Repository<Category>()
                .AddAsync(mappingCategory, cancellationToken);

            //cancel token
            await unitOfWork.SaveAsync(cancellationToken);

            //commit transaction
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
        long? parentId,
        CancellationToken cancellationToken
    )
    {
        if (parentId == null || parentId <= 0)
        {
            return id.ToLower();
        }

        var parent = await unitOfWork
            .Repository<Category>()
            .FindByIdAsync((long)parentId, cancellationToken);

        if (parent == null || string.IsNullOrEmpty(parent.Path))
        {
            return id.ToLower(); // hoặc throw nếu Path bắt buộc
        }

        return $"{parent.Path}.{id.ToLower()}";
    }
}
