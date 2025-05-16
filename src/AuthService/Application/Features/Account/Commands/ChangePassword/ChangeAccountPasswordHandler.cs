using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.ChangePassword;

public class ChangeAccountPasswordHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
   : IRequestHandler<ChangeAccountPasswordCommand>
{
    public async ValueTask<Unit> Handle(
        ChangeAccountPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        long? userId = currentAccount.Id;

        if (!userId.HasValue)
        {
            throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().Build()]
            );
        }

        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
                    new GetAccountByIdWithoutIncludeSpecification(userId.Value),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().Build()]
            );

        if (!Verify(request.OldPassword, user.Password))
        {
            throw new BadRequestException(
                [
                    Messager
                       .Create<ChangeAccountPasswordCommand>(nameof(Account))
                       .Property(x => x.OldPassword!)
                       .Message(MessageType.Correct)
                       .Negative()
                       .Build(),
               ]
            );
        }

        user.SetPassword(HashPassword(request.NewPassword));

        await unitOfWork.Repository<Account>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
