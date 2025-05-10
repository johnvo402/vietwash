using System.IdentityModel.Tokens.Jwt;
using Application.Common.Auth;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Wangkanai.Detection.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions;
using SerializerExtension = JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions.SerializerExtension;
using Contracts.Application.Common.Interfaces.Services.Token;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Users.Commands.Login;

// ... other using statements

public class LoginUserHandler(
    IUnitOfWork unitOfWork,
    ITokenFactory tokenFactory,
    IDetectionService detectionService,
    ICurrentUser currentUser,
    ITokenSecurityService securityService
    ) : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async ValueTask<LoginUserResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken
    )
    {
        User user =
            await unitOfWork
                .Repository<User>()
                .FindByConditionAsync(
                    new GetUserByUsernameSpecification(request.Username!),
                    cancellationToken
                )
            ?? throw new NotFoundException(
                [Messager.Create<User>().Message(MessageType.Found).Negative().BuildMessage()]
            );
        if (!(user.Status == UserStatus.Active))
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<User>()
                        .Property(x => x.Status)
                        .Message(MessageType.Active)
                        .Negative()
                        .BuildMessage(),
                ]
            );
        }
        if ((user.Role.Name == "CUSTOMER"))
        {
            throw new BadRequestException(
                [
                    Messager
                        .Create<User>()
                        .Property(x => x.RoleId)
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
                        .Create<User>()
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

        var userToken = new UserToken()
        {
            ExpiredTime = refreshExpireTime,
            UserId = user.Id,
            FamilyId = familyId,
            UserAgent = userAgent,
            ClientIp = currentUser.ClientIp,
        };

        var accesstokenExpiredTime = tokenFactory.AccesstokenExpiredTime;

        string accessToken = tokenFactory.CreateToken(
             [
                new("family_id", familyId),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("token_type", "access")
            ],
            accesstokenExpiredTime
        );

        string refreshToken = tokenFactory.CreateToken(
            [
                new("family_id", familyId),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("token_type", "refresh")
            ],
            refreshExpireTime
        );

        userToken.RefreshToken = refreshToken;

        await unitOfWork.Repository<UserToken>().AddAsync(userToken, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);


        UserAuth value = new UserAuth()
        {
            Id = user.Id,
            Role = user.Role.Name,
            Permissions = user.Role.RolePermissions?.Select(p => p.Permission!.Key).ToList(),
        };
        var result = SerializerExtension.Serialize(value!);
        await securityService.AddSessionUserAsync(user.Id.ToString(), result.StringJson, (refreshExpireTime - DateTime.UtcNow));
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
