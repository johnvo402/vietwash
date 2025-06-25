using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.ResetPassword;

public class ResetAccountPasswordHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ResetAccountPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(
        ResetAccountPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>(false)
            .FindByConditionAsync(
                new GetAccountByIdIncludeResetPassword(command.AccountId),
                cancellationToken
            );
        if (user == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().Build()
                )
            );
        }

        IEnumerable<AccountResetPassword> resetPasswords = user.AccountResetPasswords ?? [];
        AccountResetPassword? resetPassword = resetPasswords.FirstOrDefault(x =>
            x.Token == command.Token
        );
        if (resetPassword == null)
            return Result.Failure(
                new BadRequestError(
                    "Token invalid",
                    Messager
                        .Create<AccountResetPassword>()
                        .Property(x => x.Token)
                        .Message(MessageType.Correct)
                        .Negative()
                        .Build()
                )
            );

        if (resetPassword.Expiry <= DateTimeOffset.UtcNow)
        {
            return Result.Failure(
                new BadRequestError(
                    "Token Expired",
                    Messager
                        .Create<AccountResetPassword>()
                        .Property(x => x.Token)
                        .Message(MessageType.Expired)
                        .Build()
                )
            );
        }

        if (user.Status == AccountStatus.Inactive)
        {
            return Result.Failure(
                new BadRequestError(
                    "Account Not Active",
                    Messager.Create<Account>().Message(MessageType.Active).Negative().Build()
                )
            );
        }

        user.SetPassword(HashPassword(command.Password));

        await unitOfWork.Repository<AccountResetPassword>().DeleteRangeAsync(resetPasswords);
        await unitOfWork.Repository<Account>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result.Success();
    }
}
