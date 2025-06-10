using System.IdentityModel.Tokens.Jwt;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Application.Common.Auth;
using JohnChum.SharedKernel.Extensions;
using Contracts.Application.Common.Interfaces.Services.Token;

namespace Application.Features.Accounts.Commands.Token;

public class RefreshTokenHandler(
    IUnitOfWork unitOfWork,
    ITokenFactory tokenFactory,
    ICurrentAccount currentUser,
    ITokenSecurityService securityService
) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async ValueTask<RefreshTokenResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        DecodeTokenResponse decodeToken = tokenFactory.DecodeToken(command.RefreshToken!);

        AccountToken? refresh = await unitOfWork
            .Repository<AccountToken>()
            .FindByConditionAsync(
                new GetRefreshtokenSpecification(
                    command.RefreshToken!,
                    long.Parse(decodeToken.Sub!)
                ),
                cancellationToken
            );

        IEnumerable<AccountToken> refreshTokens = await unitOfWork
            .Repository<AccountToken>()
            .ListAsync(
                new ListRefreshtokenByFamillyIdSpecification(
                    decodeToken.FamilyId!,
                    long.Parse(decodeToken.Sub!)
                ),
                new() { Sort = $"{nameof(AccountToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}" },
                cancellationToken
            );

        if (refresh == null)
        {
            await unitOfWork.Repository<AccountToken>().DeleteRangeAsync(refreshTokens);
            await unitOfWork.SaveAsync(cancellationToken);
            throw new BadRequestException(
                [
                    Messager
                        .Create<AccountToken>(nameof(Account))
                        .Property(x => x.Token!)
                        .Message(MessageType.Correct)
                        .Negative()
                        .BuildMessage(),
                ]
            );
        }

        if (refresh.Account!.Status == AccountStatus.Inactive)
        {
            throw new BadRequestException(
                [Messager.Create<Account>().Message(MessageType.Active).Negative().BuildMessage()]
            );
        }

        await unitOfWork.Repository<AccountToken>().DeleteRangeAsync(refreshTokens);
        Account user =
           await unitOfWork
               .Repository<Account>()
               .FindByConditionAsync(
                   new GetAccountByIdSpecification(long.Parse(decodeToken.Sub!)),
                   cancellationToken
               )
           ?? throw new NotFoundException(
               [Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()]
           );
        if (!(user.Status == AccountStatus.Active))
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<Account>()
                        .Property(x => x.Status)
                        .Message(MessageType.Active)
                        .Negative()
                        .BuildMessage(),
                ]
            );
        }
        var accesstokenExpiredTime = tokenFactory.AccesstokenExpiredTime;

        var accessToken = tokenFactory.CreateToken(
            [
                new(JwtRegisteredClaimNames.Sub.ToString(), user.Id.ToString()),
                new("family_id", decodeToken.FamilyId!),
                new("token_type", "access")
            ],
            accesstokenExpiredTime
        );

        var refreshTokenExpiredTime = tokenFactory.RefreshtokenExpiredTime;

        string refreshToken = tokenFactory.CreateToken(
            [
                new(JwtRegisteredClaimNames.Sub.ToString(), decodeToken.Sub!.ToString()),
                new("family_id", decodeToken.FamilyId!),
                new("token_type", "refresh")
            ],
            refreshTokenExpiredTime
        );

        var userToken = new AccountToken()
        {
            FamilyId = decodeToken.FamilyId,
            AccountId = long.Parse(decodeToken.Sub!),
            ExpiredTime = refreshTokenExpiredTime,
            Token = refreshToken,
            ClientIp = currentUser.ClientIp,
        };

        await unitOfWork.Repository<AccountToken>().AddAsync(userToken, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        var branches = user.BranchAccounts?.Select(x => x.BranchId.ToString()) ?? [];
        UserAuth value = new UserAuth() { Id = user.Id, Role = user.Role, Branches = branches };
        var result = SerializerExtension.Serialize(value!);
        await securityService.AddSessionUserAsync(
            user.Id.ToString(),
            result.StringJson,
            (refreshTokenExpiredTime - DateTime.UtcNow)
        );

        return new() { Token = accessToken, Refresh = refreshToken, AccessTokenExpiredIn = new DateTimeOffset(accesstokenExpiredTime).ToUnixTimeMilliseconds(), TokenType = JwtBearerDefaults.AuthenticationScheme };
    }
}
