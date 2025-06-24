using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.Delete;

public class DeleteAccountHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAccountCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdWithoutIncludeSpecification(command.AccountId),
                cancellationToken
            );
        if (user == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "The source not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }
        user.Disabled = true;
        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Repository<Account>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        return Result.Success();
    }
}
