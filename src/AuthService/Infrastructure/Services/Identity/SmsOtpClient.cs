using System.Data.Common;
using System.Text;
using System.Text.Json;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Utils;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Otp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Infrastructure.Services.Identity
{
    public class SmsOtpClient : ISmsOtpClient
    {
        private readonly OtpOption _otpOption;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentAccount _currentAccount;
        private readonly ILogger _logger;
        private readonly IRedisCacheService _cache;
        private static readonly Random _random = new();

        private const int OtpLength = 6;
        private const int OtpExpirationMinutes = 10;

        public SmsOtpClient(
            IOptions<OtpOption> otpOption,
            HttpClient httpClient,
            IUnitOfWork unitOfWork,
            ICurrentAccount currentAccount,
            ILogger logger,
            IRedisCacheService redisCache
        )
        {
            _otpOption = otpOption?.Value ?? throw new ArgumentNullException(nameof(otpOption));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentAccount =
                currentAccount ?? throw new ArgumentNullException(nameof(currentAccount));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));

            ConfigureHttpClient();
        }

        public async Task<CreatePinResponse> CreatePinAsync(
            CreatePinRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ValidateCreatePinRequest(request);

            string otpCode = await GenerateAndSendOtpAsync(request, cancellationToken);

            return request.AccountId.HasValue
                ? await StoreOtpForAccountAsync(request, otpCode, cancellationToken)
                : await StoreOtpInCacheAsync(otpCode, cancellationToken);
        }

        public async Task<bool> VerifyPinAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ValidateVerifyPinRequest(request);

            return request.AccountId.HasValue
                ? await VerifyAccountOtpAsync(request, cancellationToken)
                : await VerifyCachedOtpAsync(request, cancellationToken);
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_otpOption.DomainUrl!);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Authorization",
                _otpOption.ApiKey
            );
        }

        private void ValidateCreatePinRequest(CreatePinRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(request.To))
            {
                throw new ArgumentException(
                    "Recipient phone number cannot be empty.",
                    nameof(request.To)
                );
            }
        }

        private void ValidateVerifyPinRequest(VerifyPinRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(request.Otp))
            {
                throw new ArgumentException("OTP cannot be empty.", nameof(request.Otp));
            }
        }

        private async Task<string> GenerateAndSendOtpAsync(
            CreatePinRequest request,
            CancellationToken cancellationToken
        )
        {
            string code = GenerateOtpCode();
            var payload = new Dictionary<string, string>
            {
                { "to", request.To },
                { "message", $"[VietWash] Your verification code is: {code}" },
            };

            _logger.Information("Sending SMS OTP: to={To}, code={Code}", request.To, code);

            try
            {
                using var content = ToJsonContent(payload);
                var response = await _httpClient.PostAsync(
                    string.Empty,
                    content,
                    cancellationToken
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error("Failed to send SMS OTP: {ReasonPhrase}", response.ReasonPhrase);
                    throw new HttpRequestException(
                        $"Failed to send SMS OTP: {response.ReasonPhrase}"
                    );
                }

                return code;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error creating OTP for AccountId={AccountId}",
                    request.AccountId
                );
                throw new Exception("Failed to send OTP.", ex);
            }
        }

        private async Task<CreatePinResponse> StoreOtpForAccountAsync(
            CreatePinRequest request,
            string otpCode,
            CancellationToken cancellationToken
        )
        {
            using var transaction = await _unitOfWork.CreateTransactionAsync(cancellationToken);
            try
            {
                var expiresAt = DateTimeOffset
                    .UtcNow.AddMinutes(OtpExpirationMinutes)
                    .ToUnixTimeSeconds();

                await _unitOfWork
                    .Repository<AccountActivity>()
                    .AddAsync(
                        new AccountActivity
                        {
                            AccountId = request.AccountId!.Value,
                            Type = request.Type,
                            Ip = _currentAccount.ClientIp,
                            Metadata = new Dictionary<string, string>
                            {
                                { "to", request.To },
                                { "code", otpCode },
                                { "expiresAt", expiresAt.ToString() },
                            },
                        },
                        cancellationToken
                    );

                await _unitOfWork.SaveAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.Information(
                    "Successfully created and stored OTP for AccountId={AccountId}",
                    request.AccountId
                );
                return new CreatePinResponse { Key = null };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.Error(ex, "Error storing OTP for AccountId={AccountId}", request.AccountId);
                throw new Exception("Failed to store OTP.", ex);
            }
        }

        private async Task<CreatePinResponse> StoreOtpInCacheAsync(
            string otpCode,
            CancellationToken cancellationToken
        )
        {
            string key = Generator.GenerateRandomString(6);
            await _cache.Database.StringSetAsync(
                key,
                otpCode,
                TimeSpan.FromMinutes(OtpExpirationMinutes)
            );
            return new CreatePinResponse { Key = key };
        }

        private async Task<bool> VerifyAccountOtpAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken
        )
        {
            var accountActivity = await _unitOfWork
                .Repository<AccountActivity>()
                .FindByConditionAsync(
                    new GetAccountActivitySpecification(request.AccountId!.Value, request.Type),
                    cancellationToken
                );

            if (accountActivity == null)
            {
                _logger.Warning(
                    "No account activity found for AccountId={AccountId}, Type={Type}",
                    request.AccountId,
                    request.Type
                );
                return false;
            }

            if (accountActivity.Ip != _currentAccount.ClientIp)
            {
                _logger.Warning("IP mismatch for AccountId={AccountId}", request.AccountId);
                return false;
            }

            if (!accountActivity.Metadata!.TryGetValue("code", out var code) || code != request.Otp)
            {
                _logger.Warning("Invalid OTP for AccountId={AccountId}", request.AccountId);
                return false;
            }

            if (
                !accountActivity.Metadata.TryGetValue("expiresAt", out var expiresAt)
                || !long.TryParse(expiresAt, out var expiresAtLong)
                || expiresAtLong < DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            )
            {
                _logger.Warning("OTP expired for AccountId={AccountId}", request.AccountId);
                return false;
            }

            _logger.Information(
                "OTP verified successfully for AccountId={AccountId}",
                request.AccountId
            );
            return true;
        }

        private async Task<bool> VerifyCachedOtpAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken
        )
        {
            var otp = await _cache.Database.StringGetAsync(request.Key);
            bool check = otp.HasValue && otp == request.Otp;
            if (check)
            {
                await _cache.Database.KeyDeleteAsync(request.Key);
            }
            return check;
        }

        private static string GenerateOtpCode()
        {
            int maxValue = (int)Math.Pow(10, OtpLength) - 1;
            int minValue = (int)Math.Pow(10, OtpLength - 1);
            return _random.Next(minValue, maxValue + 1).ToString($"D{OtpLength}");
        }

        private static StringContent ToJsonContent<T>(T data) =>
            new(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
    }
}
