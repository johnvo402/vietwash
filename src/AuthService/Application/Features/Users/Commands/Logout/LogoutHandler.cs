using System.IdentityModel.Tokens.Jwt;
using Mediator;
using Contracts.Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.Services.Token;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Users;
using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Models;
namespace Application.Features.Users.Commands.Logout;

public class LogoutHandler(IBlacklistTokenService blacklistService,
    ITokenFactory tokenFactory,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand, LogoutResponse>
{
    public async ValueTask<LogoutResponse> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!string.IsNullOrEmpty(command.Token))
        {
            DecodeTokenResponse decodeToken = tokenFactory.DecodeToken(command.Token!);

            IEnumerable<UserToken> refreshTokens = await unitOfWork
                .Repository<UserToken>()
            .ListAsync(
            new ListRefreshtokenByFamillyIdSpecification(
                    decodeToken.FamilyId!,
                    Ulid.Parse(decodeToken.Sub!)
                    ),
                    new() { Sort = $"{nameof(UserToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}" },
                    cancellationToken
                );
            if (refreshTokens != null && refreshTokens.Any())
            {
                await unitOfWork.Repository<UserToken>().DeleteRangeAsync(refreshTokens);

                await unitOfWork.SaveAsync(cancellationToken);

            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadToken(command.Token) as JwtSecurityToken;

            if (jwtToken != null)
            {
                var expiry = jwtToken.ValidTo - DateTime.UtcNow;
                await blacklistService.AddToBlacklistAsync(command.Token, expiry);
            }

        }

        return new LogoutResponse { Message = "Logged out successfully" };
    }
}
