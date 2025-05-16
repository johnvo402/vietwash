using System.IdentityModel.Tokens.Jwt;
using Application.Common.Auth;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Token;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Wangkanai.Detection.Services;
using SerializerExtension = JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions.SerializerExtension;

namespace Application.Features.Accounts.Commands.Login;

// ... other using statements

public class LoginAccountHandler(
    IUnitOfWork unitOfWork,
    ITokenFactory tokenFactory,
    IDetectionService detectionService,
    ICurrentAccount currentAccount,
    ITokenSecurityService securityService
) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async ValueTask<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
    {
        Account user =
            await unitOfWork
                .Repository<Account>()
                .FindByConditionAsync(
                    new GetAccountByEmailSpecification(request.Email!),
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
        if ((user.Role == "CUSTOMER"))
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<Account>()
                        .Property(x => x.Role)
                        .Message(MessageType.Valid)
                        .Negative()
                        .BuildMessage(),
                ]
            );
        }
        if (!Verify(request.Password, user.Password))
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<Account>()
                        .Property(x => x.Password)
                        .Message(MessageType.Correct)
                        .Negative()
                        .BuildMessage(),
                ]
            );
        }

        DateTime refreshExpireTime = tokenFactory.RefreshtokenExpiredTime;
        string familyId = StringExtension.GenerateRandomString(32);

        var userAgent = detectionService.UserAgent.ToString();

        var userToken = new AccountToken()
        {
            ExpiredTime = refreshExpireTime,
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

        UserAuth value = new UserAuth() { Id = user.Id, Role = user.Role };
        var result = SerializerExtension.Serialize(value!);
        await securityService.AddSessionUserAsync(
            user.Id.ToString(),
            result.StringJson,
            (refreshExpireTime - DateTime.UtcNow)
        );
        return new()
        {
            Token = accessToken,
            Refresh = refreshToken,
            AccessTokenExpiredIn = (long)
                Math.Ceiling((accesstokenExpiredTime - DateTime.UtcNow).TotalSeconds),
            TokenType = JwtBearerDefaults.AuthenticationScheme,
        };
    }
}
