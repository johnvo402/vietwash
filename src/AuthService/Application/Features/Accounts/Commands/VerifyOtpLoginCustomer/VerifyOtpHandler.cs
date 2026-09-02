using System.IdentityModel.Tokens.Jwt;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Extensions;
using Contracts.Utils;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Otp;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Shared.Kernel.Extensions;

namespace Application.Features.Accounts.Commands.VerifyOtpLoginCustomer
{
    public class VerifyOtpHandler(
        ICurrentAccount _currentAccount,
        ISmsOtpClient _client,
        IUnitOfWork _unitOfWork,
        ITokenFactory _tokenFactory,
        ITokenSecurityService _securityService
    ) : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpResponse>>
    {
        public async ValueTask<Result<VerifyOtpResponse>> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken
        )
        {
            var verifyRequest = new VerifyPinRequest
            {
                To = request.PhoneNumber,
                Otp = request.Otp,
                ClientIp = _currentAccount.ClientIp!,
            };

            // Verify OTP
            bool isValid = await _client.VerifyAsync(verifyRequest, cancellationToken);
            if (!isValid)
            {
                return Result<VerifyOtpResponse>.Success(
                    new VerifyOtpResponse { Verified = false }
                );
            }

            // Check for existing account
            Account? user = await _unitOfWork
                .DynamicReadOnlyRepository<Account>()
                .FindByConditionAsync(
                    new GetAccountByPhoneNumberSpecification(request.PhoneNumber, ROLE.CUSTOMER),
                    cancellationToken
                );

            bool isNewAccount = user is null;
            bool requiresSynchronization = user is null || !user.Verified;

            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            bool committed = false;
            try
            {
                if (isNewAccount)
                {
                    user = new Account(
                        request.PhoneNumber,
                        null,
                        null,
                        request.PhoneNumber,
                        ROLE.CUSTOMER,
                        Generator.GenerateAccountCode(ROLE.CUSTOMER)
                    );

                    user.VerifiedCustomer();
                    user.CreateAccount();
                    user = await _unitOfWork
                        .Repository<Account>()
                        .AddAsync(user, cancellationToken);
                }
                else if (requiresSynchronization)
                {
                    user!.VerifiedCustomer();
                    user.CreateAccount();
                    await _unitOfWork.Repository<Account>().UpdateAsync(user);
                }

                // Generate tokens
                string familyId = StringExtension.GenerateRandomString(32);
                var accessTokenExpireTime = _tokenFactory.AccesstokenExpiredTime;
                var refreshExpireTime = DateTime.UtcNow.AddDays(15);
                string accessToken = _tokenFactory.CreateToken(
                    [
                        new("family_id", familyId),
                        new(JwtRegisteredClaimNames.Sub, user!.Id.ToString()),
                        new("token_type", "access"),
                    ],
                    accessTokenExpireTime
                );

                string refreshToken = _tokenFactory.CreateToken(
                    [
                        new("family_id", familyId),
                        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new("token_type", "refresh"),
                    ],
                    refreshExpireTime
                );

                // Store refresh token
                var userToken = new AccountToken
                {
                    ExpiredTime = 15,
                    AccountId = user.Id,
                    FamilyId = familyId,
                    ClientIp = verifyRequest.ClientIp,
                    Token = refreshToken,
                };

                await _unitOfWork.Repository<AccountToken>().AddAsync(userToken, cancellationToken);

                // Save session
                var branches =
                    user.BranchAccounts?.Select(x => x.BranchId.ToString())
                    ?? Array.Empty<string>();
                var userAuth = new UserAuth
                {
                    Id = user.Id,
                    Role = user.Role,
                    Branches = branches,
                };

                var result = SerializerExtension.Serialize(userAuth);
                await _unitOfWork.SaveAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                await _securityService.AddSessionUserAsync(
                    user.Id.ToString(),
                    result.StringJson,
                    refreshExpireTime - DateTime.UtcNow
                );

                return Result<VerifyOtpResponse>.Success(
                    new VerifyOtpResponse
                    {
                        IsNew = isNewAccount,
                        Verified = true,
                        Token = accessToken,
                        Refresh = refreshToken,
                        AccessTokenExpiredIn = new DateTimeOffset(
                            accessTokenExpireTime
                        ).ToUnixTimeSeconds(),
                        TokenType = JwtBearerDefaults.AuthenticationScheme,
                    }
                );
            }
            catch (Exception)
            {
                if (!committed)
                    await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
