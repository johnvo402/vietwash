using System.Data.Common;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using Mediator;

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
        if (user.Role == ROLE.CUSTOMER)
        {
            user.VerifiedCustomer();
        }
        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);
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

        return new UpdateAccountProfileResponse { Message = "Success" };
    }
}
