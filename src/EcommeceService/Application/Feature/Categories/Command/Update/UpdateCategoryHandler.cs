using System.Data.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Categories.Command.Update;

public class UpdateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateCategoryCommand>
{
    public async ValueTask<Mediator.Unit> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Category? getCategory =
            await unitOfWork.Repository<Category>().FindByIdAsync(Ulid.Parse(command.CategoryId))
            ?? throw new NotFoundException(
                [Messager.Create<Category>().Message(MessageType.Found).Negative().BuildMessage()]
            );
        mapper.Map(command.Category, getCategory);

        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Category>().UpdateAsync(getCategory);

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return Mediator.Unit.Value;
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
