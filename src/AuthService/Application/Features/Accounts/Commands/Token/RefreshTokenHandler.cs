using System.IdentityModel.Tokens.Jwt;
using Application.Common.Auth;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Shared.Kernel.Extensions;
using Wangkanai.Detection.Services;

namespace Application.Features.Accounts.Commands.Token;

public class RefreshTokenHandler(
    IUnitOfWork unitOfWork,
    ITokenFactory tokenFactory,
    ICurrentAccount currentUser,
    ITokenSecurityService securityService
) : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async ValueTask<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        DecodeTokenResponse decodeToken = tokenFactory.DecodeToken(command.RefreshToken!);

        AccountToken? refresh = await unitOfWork
            .DynamicReadOnlyRepository<AccountToken>()
            .FindByConditionAsync(
                new GetRefreshtokenSpecification(
                    command.RefreshToken!,
                    long.Parse(decodeToken.Sub!)
                ),
                cancellationToken
            );

        IEnumerable<AccountToken> refreshTokens = await unitOfWork
            .DynamicReadOnlyRepository<AccountToken>()
            .ListAsync(
                new ListRefreshtokenByFamillyIdSpecification(
                    decodeToken.FamilyId!,
                    long.Parse(decodeToken.Sub!)
                ),
                new()
                {
                    Sort = $"{nameof(AccountToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                },
                cancellationToken
            );

        if (refresh == null)
        {
            await unitOfWork.Repository<AccountToken>().DeleteRangeAsync(refreshTokens);
            await unitOfWork.SaveAsync(cancellationToken);
            return Result<RefreshTokenResponse>.Failure(
                new BadRequestError(
                    "Token invalid",
                    Messager
                        .Create<AccountToken>(nameof(Account))
                        .Property(x => x.Token!)
                        .Message(MessageType.Correct)
                        .Negative()
                        .BuildMessage()
                )
            );
        }

        await unitOfWork.Repository<AccountToken>().DeleteRangeAsync(refreshTokens);
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByIdSpecification(long.Parse(decodeToken.Sub!)),
                cancellationToken
            );

        if (user == null)
        {
            return Result<RefreshTokenResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        if (!(user.Status == AccountStatus.Active))
        {
            return Result<RefreshTokenResponse>.Failure(
                new BadRequestError(
                    "Account inactive",
                    Messager.Create<Account>().Message(MessageType.Active).Negative().BuildMessage()
                )
            );
        }
        var accesstokenExpiredTime = tokenFactory.AccesstokenExpiredTime;

        var accessToken = tokenFactory.CreateToken(
            [
                new(JwtRegisteredClaimNames.Sub.ToString(), user.Id.ToString()),
                new("family_id", decodeToken.FamilyId!),
                new("token_type", "access"),
            ],
            accesstokenExpiredTime
        );

        var refreshTokenExpiredTime = DateTime.UtcNow.AddDays(refresh.ExpiredTime);

        string refreshToken = tokenFactory.CreateToken(
            [
                new(JwtRegisteredClaimNames.Sub.ToString(), decodeToken.Sub!.ToString()),
                new("family_id", decodeToken.FamilyId!),
                new("token_type", "refresh"),
            ],
            refreshTokenExpiredTime
        );

        var userToken = new AccountToken()
        {
            FamilyId = decodeToken.FamilyId,
            AccountId = long.Parse(decodeToken.Sub!),
            ExpiredTime = refresh.ExpiredTime,
            Token = refreshToken,
            ClientIp = currentUser.ClientIp,
        };

        await unitOfWork.Repository<AccountToken>().AddAsync(userToken, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        var branches = user.BranchAccounts?.Select(x => x.BranchId.ToString()) ?? [];
        UserAuth value = new UserAuth()
        {
            Id = user.Id,
            Role = user.Role,
            Branches = branches,
        };
        var result = SerializerExtension.Serialize(value!);
        await securityService.AddSessionUserAsync(
            user.Id.ToString(),
            result.StringJson,
            refreshTokenExpiredTime - DateTime.UtcNow
        );

        return Result<RefreshTokenResponse>.Success(
            new()
            {
                Token = accessToken,
                Refresh = refreshToken,
                AccessTokenExpiredIn = new DateTimeOffset(
                    accesstokenExpiredTime
                ).ToUnixTimeMilliseconds(),
                TokenType = JwtBearerDefaults.AuthenticationScheme,
            }
        );
    }
}
