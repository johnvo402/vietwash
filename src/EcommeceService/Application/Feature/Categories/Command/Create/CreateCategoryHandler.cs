using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Categories.Command.Create;

public class CreateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateCategoryCommand>
{
    public async ValueTask<Mediator.Unit> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Category mappingCategory = mapper.Map<Category>(command);
        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            //add into database
            Category category = await unitOfWork
                .Repository<Category>()
                .AddAsync(mappingCategory, cancellationToken);

            //cancel token
            await unitOfWork.SaveAsync(cancellationToken);

            //commit transaction
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
