using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Categories.Command.Delete;

public class DeleteCategoryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Category? getCategory = await unitOfWork
            .Repository<Category>()
            .FindByIdAsync(long.Parse(command.CategoryId), cancellationToken);
        if (getCategory == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "category not found",
                    Messager.Create<Category>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        getCategory.Disabled = true;
        await unitOfWork.Repository<Category>().UpdateAsync(getCategory);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result.Success();
    }
}
