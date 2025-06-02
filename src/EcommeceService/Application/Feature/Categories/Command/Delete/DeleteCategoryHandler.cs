using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Categories.Command.Delete;

public class DeleteCategoryHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand>
{
    public async ValueTask<Mediator.Unit> Handle(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Category? getCategory =
            await unitOfWork.Repository<Category>().FindByIdAsync(command.CategoryId)
            ?? throw new NotFoundException(
                [Messager.Create<Category>().Message(MessageType.Found).Negative().BuildMessage()]
            );
        getCategory.Disabled = true;
        await unitOfWork.Repository<Category>().UpdateAsync(getCategory);
        await unitOfWork.SaveAsync(cancellationToken);

        return Mediator.Unit.Value;
    }
}
