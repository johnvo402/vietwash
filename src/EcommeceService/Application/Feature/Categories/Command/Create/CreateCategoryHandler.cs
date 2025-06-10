using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Contracts.Utils;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
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
        if (string.IsNullOrEmpty(command.Id))
        {
            command.Id = Generator.GenerateCode("CT-", 6);
        }
        Category mappingCategory = mapper.Map<Category>(command);
        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            //add into database
            Category category = await unitOfWork
                .Repository<Category>()
                .AddAsync(mappingCategory, cancellationToken);

            category.Path = await GenerateCategoryPathAsync(
                category.Id,
                command.ParentId,
                cancellationToken
            );
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

    private async Task<string> GenerateCategoryPathAsync(string id, string? parentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(parentId))
        {
            return id;
        }

        var parent = await unitOfWork.Repository<Category>().FindByIdAsync(parentId, cancellationToken);

        if (parent == null)
            throw new NotFoundException(
               [Messager.Create<Category>().Message(MessageType.Found).Negative().BuildMessage()]
           );

        return $"{parent.Path}.{id.ToLower()}";
    }
}
