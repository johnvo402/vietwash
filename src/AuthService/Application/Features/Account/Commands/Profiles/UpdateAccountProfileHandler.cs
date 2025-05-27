using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;

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
new GetAccountByIdSpecification(currentAccount.Id!.Value),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
            );

        string? oldAvatar = user.AvtUrl;

        mapper.Map(command, user);

        user.AvtUrl = command.AvtUrl;

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

        return new UpdateAccountProfileResponse
        {
            Message= "Success",
        };
    }
}
