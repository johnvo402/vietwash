using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Token;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using JohnChum.SharedKernel.Extensions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Features.Accounts.Queries.Profiles;

public class GetAccountProfileHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    ITokenSecurityService securityService
) : IRequestHandler<GetAccountProfileQuery, GetAccountProfileResponse>
{
    public async ValueTask<GetAccountProfileResponse> Handle(
        GetAccountProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        GetAccountProfileResponse account =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync<GetAccountProfileResponse>(
                    new GetAccountByIdSpecification(currentAccount.Id!.Value),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
            );

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

        return account;
    }
}
