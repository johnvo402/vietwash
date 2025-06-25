using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Utils;
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
        if (string.IsNullOrEmpty(command.Id))
        {
            command.Id = Generator.GenerateCode("CT-", 6);
        }
        Category mappingCategory = command.ToEntity();
        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            mappingCategory.Path = await GenerateCategoryPathAsync(
                mappingCategory.Id,
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

        if (parent == null || string.IsNullOrEmpty(parent.Path))
        {
            return id.ToLower(); // hoặc throw nếu Path bắt buộc
        }

        return $"{parent.Path}.{id.ToLower()}";
    }
}
