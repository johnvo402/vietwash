using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    IMapper mapper,
    IMediaUpdateService<Account> avatarUpdate
) : IRequestHandler<UpdateAccountProfileCommand, UpdateAccountProfileResponse>
{
    public async ValueTask<UpdateAccountProfileResponse> Handle(
        UpdateAccountProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
new GetAccountByIdWithoutIncludeSpecification(currentAccount.Id!.Value),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
            );

        IFormFile? avatar = command.Avatar;
        string? oldAvatar = user.AvtUrl;

        mapper.Map(command, user);

        string? key = avatarUpdate.GetKey(avatar);
        user.AvtUrl = await avatarUpdate.UploadAvatarAsync(avatar, key);

        try
        {
            await unitOfWork.Repository<Account>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);
            await avatarUpdate.DeleteAvatarAsync(oldAvatar);
        }
        catch (Exception)
        {
            await avatarUpdate.DeleteAvatarAsync(user.AvtUrl);
            throw;
        }

        return (
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync<UpdateAccountProfileResponse>(
                    new GetAccountByIdSpecification(user.Id),
                    cancellationToken
                )
        )!;
    }
}
