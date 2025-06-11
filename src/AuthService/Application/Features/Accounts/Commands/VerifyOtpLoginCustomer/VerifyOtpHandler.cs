using System.IdentityModel.Tokens.Jwt;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Utils;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Wangkanai.Detection.Services;

namespace Application.Features.Accounts.Commands.VerifyOtpLoginCustomer
{
    public class VerifyOtpHandler(
        ICurrentAccount currentAccount,
        ISmsOtpClient _client,
        IUnitOfWork unitOfWork,
        ITokenFactory tokenFactory,
        IDetectionService detectionService,
        ITokenSecurityService securityService
    ) : IRequestHandler<VerifyOtpCommand, VerifyOtpResponse>
    {
        public async ValueTask<VerifyOtpResponse> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken
        )
        {
            var check = await _client.VerifyPinAsync(request);
            bool isNew = request.Key != null;
            Account? user;

            if (!check)
                return new() { Verified = false };
            string accessToken = string.Empty;
            string refreshToken = string.Empty;
            DateTime refreshExpireTime = tokenFactory.RefreshtokenExpiredTime;
            string familyId = StringExtension.GenerateRandomString(32);
            string userAgent = detectionService.UserAgent.ToString();

            var accessTokenExpireTime = tokenFactory.AccesstokenExpiredTime;

            using var transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);
            try
            {
                if (isNew)
                {
                    Account account = new Account(
                        request.PhoneNumber,
                        null,
                        null,
                        request.PhoneNumber,
                        ROLE.CUSTOMER,
                        Generator.GenerateAccountCode(ROLE.CUSTOMER)
                    );
                    user = await unitOfWork
                        .Repository<Account>()
                        .AddAsync(account, cancellationToken);
                    await unitOfWork.SaveAsync(cancellationToken);
                }
                else
                {
                    user = await unitOfWork
                        .Repository<Account>()
                        .FindByConditionAsync(
                            new GetAccountByIdSpecification((long)request.AccountId!),
                            cancellationToken
                        );
                }

                // Create tokens
                accessToken = tokenFactory.CreateToken(
                    [
                        new("family_id", familyId),
                        new(JwtRegisteredClaimNames.Sub, user!.Id.ToString()),
                        new("token_type", "access"),
                    ],
                    accessTokenExpireTime
                );

                refreshToken = tokenFactory.CreateToken(
                    [
                        new("family_id", familyId),
                        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new("token_type", "refresh"),
                    ],
                    refreshExpireTime
                );

                var userToken = new AccountToken()
                {
                    ExpiredTime = refreshExpireTime,
                    AccountId = user.Id,
                    FamilyId = familyId,
                    ClientIp = currentAccount.ClientIp,
                    Token = refreshToken,
                };

                await unitOfWork.Repository<AccountToken>().AddAsync(userToken, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            var branches = user!.BranchAccounts?.Select(x => x.BranchId.ToString()) ?? [];
            var userAuth = new UserAuth
            {
                Id = user.Id,
                Role = user.Role,
                Branches = branches,
            };

            var result = SerializerExtension.Serialize(userAuth);
            await securityService.AddSessionUserAsync(
                user.Id.ToString(),
                result.StringJson,
                refreshExpireTime - DateTime.UtcNow
            );

            return new()
            {
                IsNew = !user.Verified,
                Verified = user.Verified,
                Token = accessToken,
                Refresh = refreshToken,
                AccessTokenExpiredIn = new DateTimeOffset(
                    accessTokenExpireTime
                ).ToUnixTimeMilliseconds(),
                TokenType = JwtBearerDefaults.AuthenticationScheme,
            };
        }
    }
}
