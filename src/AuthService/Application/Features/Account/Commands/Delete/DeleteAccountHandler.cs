using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.Delete;

public class DeleteAccountHandler(IUnitOfWork unitOfWork, IMediaUpdateService<Account> MediaUpdateService)
    : IRequestHandler<DeleteAccountCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
                    new GetAccountByIdWithoutIncludeSpecification(command.AccountId),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
            );
        string? avatar = user.AvtUrl;
        await unitOfWork.Repository<Account>().DeleteAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        await MediaUpdateService.DeleteAvatarAsync(avatar);
        return Unit.Value;
    }
}
