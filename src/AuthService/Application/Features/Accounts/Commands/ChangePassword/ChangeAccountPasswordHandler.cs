using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.ChangePassword;

public class ChangeAccountPasswordHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<ChangeAccountPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(
        ChangeAccountPasswordCommand request,
        CancellationToken cancellationToken
    )
    {
        long? userId = currentAccount.Id;
        if (userId == null)
        {
            return Result.Failure(new UnauthorizedError(Message.UNAUTHORIZED));
        }
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdWithoutIncludeSpecification((long)userId),
                cancellationToken
            );

        if (user == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "The resource is not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().Build()
                )
            );
        }

        if (!Verify(request.OldPassword, user.Password))
        {
            return Result.Failure(
                new BadRequestError(
                    "Error has occured with password",
                    Messager
                        .Create<ChangeAccountPasswordCommand>(nameof(Account))
                        .Property(x => x.OldPassword!)
                        .Message(MessageType.Correct)
                        .Negative()
                        .Build()
                )
            );
        }

        user.SetPassword(HashPassword(request.NewPassword));

        await unitOfWork.Repository<Account>().UpdateAsync(user);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result.Success();
    }
}
