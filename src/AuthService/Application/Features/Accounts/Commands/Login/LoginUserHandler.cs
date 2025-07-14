using System.IdentityModel.Tokens.Jwt;
using Application.Common.Auth;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Common.Messages;
using Contracts.Extensions;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Shared.Kernel.Extensions;
using Wangkanai.Detection.Services;

namespace Application.Features.Accounts.Commands.Login;

// ... other using statements

public class LoginAccountHandler(
    IUnitOfWork unitOfWork,
    ITokenFactory tokenFactory,
    IDetectionService detectionService,
    ICurrentAccount currentAccount,
    ITokenSecurityService securityService
) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
    {
        Account? user = await unitOfWork
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(
                new GetAccountByEmailSpecification(request.Email!),
                cancellationToken
            );
        if (user == null)
        {
            return Result<LoginResponse>.Failure(
                new NotFoundError(
                    "Account not found",
                    Messager.Create<Account>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        if (!(user.Status == AccountStatus.Active))
        {
            return Result<LoginResponse>.Failure(
                new BadRequestError(
                    "Account not active",
                    Messager
                        .Create<Account>()
                        .Property(x => x.Status)
                        .Message(MessageType.Active)
                        .Negative()
                        .BuildMessage()
                )
            );
        }
        if (!Verify(request.Password, user.Password))
        {
            return Result<LoginResponse>.Failure(
                new BadRequestError(
                    "Password not correct",
                    Messager
                        .Create<Account>()
                        .Property(x => x.Password!)
                        .Message(MessageType.Correct)
                        .Negative()
                        .BuildMessage()
                )
            );
        }

        DateTime refreshExpireTime = tokenFactory.RefreshtokenExpiredTime;
        string familyId = StringExtension.GenerateRandomString(32);
        if (request.RememberMe == true)
            refreshExpireTime = DateTime.UtcNow.AddDays(7);
        var userAgent = detectionService.UserAgent.ToString();
        var expiredTime = refreshExpireTime.Day - DateTime.UtcNow.Day;
        var userToken = new AccountToken()
        {
            ExpiredTime = expiredTime,
            AccountId = user.Id,
            FamilyId = familyId,
            ClientIp = currentAccount.ClientIp,
        };

        var accesstokenExpiredTime = tokenFactory.AccesstokenExpiredTime;

        string accessToken = tokenFactory.CreateToken(
            [
                new("family_id", familyId),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("token_type", "access"),
            ],
            accesstokenExpiredTime
        );

        string refreshToken = tokenFactory.CreateToken(
            [
                new("family_id", familyId),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("token_type", "refresh"),
            ],
            refreshExpireTime
        );

        userToken.Token = refreshToken;

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
            refreshExpireTime - DateTime.UtcNow
        );
        return Result<LoginResponse>.Success(
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
