using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.ResetPassword;

public class ResetAccountPasswordHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ResetAccountPasswordCommand>
{
    public async ValueTask<Unit> Handle(
        ResetAccountPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
                    new GetAccountByIdIncludeResetPassword(command.AccountId),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().Build()]
            );

        IEnumerable<AccountResetPassword> resetPasswords = user.AccountResetPasswords ?? [];
        AccountResetPassword? resetPassword =
            resetPasswords.FirstOrDefault(x => x.Token == command.Token)
            ?? throw new BadRequestException(
                [
                    Messager
                        .Create<AccountResetPassword>()
                        .Property(x => x.Token)
                        .Message(MessageType.Correct)
                        .Negative()
                        .Build(),
                ]
            );

        if (resetPassword.Expiry <= DateTimeOffset.UtcNow)
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<AccountResetPassword>()
                        .Property(x => x.Token)
                        .Message(MessageType.Expired)
                        .Build(),
                ]
            );
        }

        if (user.Status == AccountStatus.Inactive)
        {
            throw new BadRequestException(
                [Messager.Create<Account>().Message(MessageType.Active).Negative().Build()]
            );
        }

        user.SetPassword(HashPassword(command.Password));

        await unitOfWork.Repository<AccountResetPassword>().DeleteRangeAsync(resetPasswords);
        await unitOfWork.Repository<Account>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
