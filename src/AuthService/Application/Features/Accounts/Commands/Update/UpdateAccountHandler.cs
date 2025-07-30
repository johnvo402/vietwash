using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountHandler(IUnitOfWork unitOfWork, IMediaUpdateService mediaUpdateService)
    : IRequestHandler<UpdateAccountCommand, Result<UpdateAccountResponse>>
{
    public async ValueTask<Result<UpdateAccountResponse>> Handle(
        UpdateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdSpecification(command.AccountId),
                cancellationToken
            );
        if (user == null)
        {
            return Result<UpdateAccountResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        string? oldAvatar = user.AvtUrl;

        user.FromUpdateUser(command.Account!);

        user.AvtUrl = command.Account!.AvtUrl;
        // update default claim

        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Account>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            await mediaUpdateService.DeleteMediaAsync(oldAvatar);
            return Result<UpdateAccountResponse>.Success(new() { Message = "Update success" });
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteMediaAsync(user.AvtUrl);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
