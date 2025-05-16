using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.Create;

public class CreateAccountHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<Account> mediaUpdateService
) : IRequestHandler<CreateAccountCommand, CreateAccountResponse>
{
    public async ValueTask<CreateAccountResponse> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        Account mappingAccount = mapper.Map<Account>(command);

        string? key = mediaUpdateService.GetKey(command.Avatar);
        mappingAccount.AvtUrl = await mediaUpdateService.UploadAvatarAsync(command.Avatar, key);

        string? userAvatar = null;
        try
        {
            
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            Account user = await unitOfWork
                .Repository<Account>()
                .AddAsync(mappingAccount, cancellationToken);
            userAvatar = user.AvtUrl;

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return (
                await unitOfWork
                    .Repository<Account>()
                    .FindByConditionAsync<CreateAccountResponse>(
                        new GetAccountByIdSpecification(user.Id),
                        cancellationToken
                    )
            )!;
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
