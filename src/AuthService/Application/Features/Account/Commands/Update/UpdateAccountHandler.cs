using System.Data.Common;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using Microsoft.AspNetCore.Http;
using Application.Features.Accounts.Commands.Update;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<Account> mediaUpdateService
) : IRequestHandler<UpdateAccountCommand, UpdateAccountResponse>
{
    public async ValueTask<UpdateAccountResponse> Handle(
        UpdateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
                    new GetAccountByIdSpecification(long.Parse(command.AccountId)),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
            );

        string? oldAvatar = user.AvtUrl;

        mapper.Map(command.Account, user);

        user.AvtUrl = command.Account!.AvtUrl;
        // update default claim

        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Account>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            await mediaUpdateService.DeleteAvatarAsync(oldAvatar);
            return mapper.Map<UpdateAccountResponse>(user);
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(user.AvtUrl);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
