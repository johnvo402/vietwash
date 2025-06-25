using Application.Common.Auth;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Common.Messages;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Shared.Kernel.Extensions;

namespace Application.Features.Accounts.Queries.Profiles;

public class GetAccountProfileHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    ITokenSecurityService securityService
) : IRequestHandler<GetAccountProfileQuery, Result<GetAccountProfileResponse>>
{
    public async ValueTask<Result<GetAccountProfileResponse>> Handle(
        GetAccountProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        GetAccountProfileResponse? account = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdSpecification(currentAccount.Id!.Value),
                x => x.ToGetAccountProfileResponse(),
                cancellationToken
            );

        if (account == null)
        {
            return Result<GetAccountProfileResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        var branches = account.BranchAccounts?.Select(x => x.BranchId.ToString()) ?? [];
        UserAuth value = new UserAuth()
        {
            Id = account.Id,
            Role = account.Role,
            Branches = branches,
        };
        var result = SerializerExtension.Serialize(value!);
        var ttl = await securityService.GetSessionExpiry(account.Id.ToString());
        if (ttl == null)
        {
            ttl = TimeSpan.FromHours(1);
        }
        await securityService.AddSessionUserAsync(
            account.Id.ToString(),
            result.StringJson,
            (TimeSpan)ttl
        );

        return Result<GetAccountProfileResponse>.Success(account);
    }
}
