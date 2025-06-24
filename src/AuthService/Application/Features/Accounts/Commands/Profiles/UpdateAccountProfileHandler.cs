using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    IMediaUpdateService<Image> avatarUpdate
) : IRequestHandler<UpdateAccountProfileCommand, Result<UpdateAccountProfileResponse>>
{
    public async ValueTask<Result<UpdateAccountProfileResponse>> Handle(
        UpdateAccountProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdSpecification(currentAccount.Id!.Value),
                cancellationToken
            );
        if (user == null)
        {
            return Result<UpdateAccountProfileResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        string? oldAvatar = user.AvtUrl;

        user.FromUpdateModel(command);

        user.AvtUrl = command.AvtUrl;
        if (user.Role == ROLE.CUSTOMER)
        {
            user.VerifiedCustomer();
        }
        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Repository<Account>().UpdateAsync(user);

            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            await avatarUpdate.DeleteAvatarAsync(oldAvatar);
        }
        catch (Exception)
        {
            await avatarUpdate.DeleteAvatarAsync(user.AvtUrl);
            await unitOfWork.RollbackAsync(cancellationToken);

            throw;
        }

        return Result<UpdateAccountProfileResponse>.Success(new() { Message = "Success" });
    }
}
