using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Mediator;

namespace Application.Features.Accounts.Commands.Logout;

public class LogoutHandler(
    ITokenSecurityService blacklistService,
    ITokenFactory tokenFactory,
    IUnitOfWork unitOfWork
) : IRequestHandler<LogoutCommand, Result<LogoutResponse>>
{
    public async ValueTask<Result<LogoutResponse>> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!string.IsNullOrEmpty(command.Token))
        {
            DecodeTokenResponse decodeToken = tokenFactory.DecodeToken(command.Token!);

            IEnumerable<AccountToken> refreshTokens = await unitOfWork
                .DynamicReadOnlyRepository<AccountToken>()
                .ListAsync(
                    new ListRefreshtokenByFamillyIdSpecification(
                        decodeToken.FamilyId!,
                        long.Parse(decodeToken.Sub!)
                    ),
                    new()
                    {
                        Sort =
                            $"{nameof(AccountToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                    },
                    cancellationToken
                );
            if (refreshTokens != null && refreshTokens.Any())
            {
                await unitOfWork.Repository<AccountToken>().DeleteRangeAsync(refreshTokens);

                await unitOfWork.SaveAsync(cancellationToken);
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadToken(command.Token) as JwtSecurityToken;

            if (jwtToken != null)
            {
                var expiry = jwtToken.ValidTo - DateTime.UtcNow;
                await blacklistService.AddToBlacklistAsync(decodeToken.FamilyId!, expiry);
            }
        }

        return Result<LogoutResponse>.Success(
            new LogoutResponse { Message = "Logged out successfully" }
        );
    }
}
