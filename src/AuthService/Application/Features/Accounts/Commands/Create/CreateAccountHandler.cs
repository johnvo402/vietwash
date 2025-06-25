using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Contracts.Utils;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.Create;

public class CreateAccountHandler(
    IUnitOfWork unitOfWork,
    IMediaUpdateService<Image> mediaUpdateService
) : IRequestHandler<CreateAccountCommand, Result<CreateAccountResponse>>
{
    public async ValueTask<Result<CreateAccountResponse>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken
    )
    {
        string code = Generator.GenerateAccountCode(command.Role);
        Account mappingAccount = command.ToAccount(code);

        string? userAvatar = null;
        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

            Account user = await unitOfWork
                .Repository<Account>()
                .AddAsync(mappingAccount, cancellationToken);
            userAvatar = user.AvtUrl;

            await unitOfWork.SaveAsync(cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            CreateAccountResponse? response = await unitOfWork
                .DynamicReadOnlyRepository<Account>()
                .FindByConditionAsync(
                    new GetAccountByIdSpecification(user.Id),
                    x => x.ToCreateAccountResponse(),
                    cancellationToken
                );
            if (response == null)
            {
                return Result<CreateAccountResponse>.Failure(
                    new BadRequestError(
                        "Create failure",
                        Messager.Create<Account>().Message(MessageType.Found).Negative().Build()
                    )
                );
            }
            return Result<CreateAccountResponse>.Success(response);
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
