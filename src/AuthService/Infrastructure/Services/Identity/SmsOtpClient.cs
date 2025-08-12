using System.Text;
using System.Text.Json;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Mail;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Otp;
using Domain.Otp.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;

namespace Infrastructure.Services.Identity
{
    public class ValidationError : ErrorDetails
    {
        public ValidationError(string title, string message)
            : base(title, message, nameof(ValidationError), StatusCodes.Status400BadRequest) { }
    }

    public class RateLimitError : ErrorDetails
    {
        public RateLimitError(string title, string message)
            : base(title, message, nameof(RateLimitError), StatusCodes.Status429TooManyRequests) { }
    }

    public class IpBlockedError : ErrorDetails
    {
        public IpBlockedError(string title, string message)
            : base(title, message, nameof(IpBlockedError), StatusCodes.Status403Forbidden) { }
    }

    public class ServiceError : ErrorDetails
    {
        public ServiceError(string title, string message)
            : base(title, message, nameof(ServiceError), StatusCodes.Status500InternalServerError)
        { }
    }

    public class OtpClient : ISmsOtpClient
    {
        private readonly OtpOption _otpOption;
        private readonly HttpClient _httpClient;
        private readonly IRedisCacheService _cache;
        private readonly ILogger _logger;
        private readonly IMailService _mailService;

        private const int OtpLength = 6;
        private const int OtpExpirationMinutes = 10;
        private const int MaxAttempts = 5;
        private const int GenerationLimitPerHour = 10;
        private const int LockoutMinutes = 30;
        private static readonly Random _random = new();

        public OtpClient(
            IOptions<OtpOption> otpOption,
            HttpClient httpClient,
            IRedisCacheService redisCache,
            ILogger logger,
            IMailService mailService
        )
        {
            _otpOption = otpOption?.Value ?? throw new ArgumentNullException(nameof(otpOption));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));

            ConfigureHttpClient();
        }

        public async Task<ErrorDetails?> CreateAsync(
            CreatePinRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var error = ValidateCreateRequest(request);
            if (error != null)
            {
                return error;
            }

            error = await CheckRateLimitAsync(
                request.To,
                request.ClientIp,
                request.Type,
                cancellationToken
            );
            if (error != null)
            {
                return error;
            }

            error = await CheckIpBlockedAsync(
                request.To,
                request.ClientIp,
                request.Type,
                cancellationToken
            );
            if (error != null)
            {
                return error;
            }

            var (otpCode, sendError) = await GenerateAndSendOtpAsync(request, cancellationToken);
            if (sendError != null)
            {
                return sendError;
            }

            await StoreOtpAsync(request, otpCode!, cancellationToken);
            return null;
        }

        public async Task<bool> VerifyAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var error = ValidateVerifyRequest(request);
            if (error != null)
            {
                _logger.Warning("Verification failed: {Error}", error.Detail);
                return false;
            }

            string attemptKey = $"otp:attempts:{request.To}:{request.ClientIp}:{request.Type}";
            error = await CheckVerificationAttemptsAsync(
                attemptKey,
                request.To,
                request.ClientIp,
                request.Type,
                cancellationToken
            );
            if (error != null)
            {
                _logger.Warning("Verification failed: {Error}", error.Detail);
                return false;
            }

            bool isValid = await VerifyOtpAsync(request, cancellationToken);
            if (isValid)
            {
                await _cache.Database.KeyDeleteAsync(attemptKey);
            }
            return isValid;
        }

        public ErrorDetails? ValidateCreateRequest(CreatePinRequest request)
        {
            if (request == null)
            {
                _logger.Warning("Validation failed: Request cannot be null.");
                return new ValidationError("Invalid request", "Request cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(request.To))
            {
                _logger.Warning("Validation failed: Recipient cannot be empty.");
                return new ValidationError("Invalid request", "Recipient cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.ClientIp))
            {
                _logger.Warning("Validation failed: Client IP cannot be empty.");
                return new ValidationError("Invalid request", "Client IP cannot be empty.");
            }
            if (request.Type != OtpType.Phone && request.Type != OtpType.Email)
            {
                _logger.Warning("Validation failed: Invalid OTP type.");
                return new ValidationError("Invalid request", "OTP type must be Phone or Email.");
            }
            return null;
        }

        public ErrorDetails? ValidateVerifyRequest(VerifyPinRequest request)
        {
            if (request == null)
            {
                _logger.Warning("Validation failed: Request cannot be null.");
                return new ValidationError("Invalid request", "Request cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(request.To))
            {
                _logger.Warning("Validation failed: Recipient cannot be empty.");
                return new ValidationError("Invalid request", "Recipient cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.Otp))
            {
                _logger.Warning("Validation failed: OTP cannot be empty.");
                return new ValidationError("Invalid request", "OTP cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.ClientIp))
            {
                _logger.Warning("Validation failed: Client IP cannot be empty.");
                return new ValidationError("Invalid request", "Client IP cannot be empty.");
            }
            if (request.Type != OtpType.Phone && request.Type != OtpType.Email)
            {
                _logger.Warning("Validation failed: Invalid OTP type.");
                return new ValidationError("Invalid request", "OTP type must be Phone or Email.");
            }
            return null;
        }

        public async Task<ErrorDetails?> CheckRateLimitAsync(
            string recipient,
            string clientIp,
            OtpType type,
            CancellationToken cancellationToken
        )
        {
            string rateLimitKey = $"otp:ratelimit:{recipient}:{clientIp}:{type}";
            var generationCount = await _cache.Database.StringIncrementAsync(rateLimitKey);
            if (generationCount == 1)
            {
                await _cache.Database.KeyExpireAsync(rateLimitKey, TimeSpan.FromHours(1));
            }

            // if (generationCount > GenerationLimitPerHour)
            // {
            //     _logger.Warning(
            //         "OTP generation limit exceeded for recipient: {Recipient}, ip: {Ip}, type: {Type}",
            //         recipient,
            //         clientIp,
            //         type
            //     );
            //     return new RateLimitError(
            //         "Rate limit exceeded",
            //         $"Too many OTP requests for this {type} and IP. Please try again later."
            //     );
            // }
            return null;
        }

        public async Task<ErrorDetails?> CheckIpBlockedAsync(
            string recipient,
            string clientIp,
            OtpType type,
            CancellationToken cancellationToken
        )
        {
            string ipBlockKey = $"otp:block:{recipient}:{clientIp}:{type}";
            if (await _cache.Database.KeyExistsAsync(ipBlockKey))
            {
                _logger.Warning(
                    "IP blocked for OTP generation for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                    recipient,
                    clientIp,
                    type
                );
                return new IpBlockedError(
                    "IP blocked",
                    $"This IP is temporarily blocked for this {type} due to suspicious activity."
                );
            }
            return null;
        }

        public async Task<ErrorDetails?> CheckVerificationAttemptsAsync(
            string attemptKey,
            string recipient,
            string clientIp,
            OtpType type,
            CancellationToken cancellationToken
        )
        {
            var attemptCount = await _cache.Database.StringIncrementAsync(attemptKey);
            if (attemptCount == 1)
            {
                await _cache.Database.KeyExpireAsync(
                    attemptKey,
                    TimeSpan.FromMinutes(OtpExpirationMinutes)
                );
            }

            if (attemptCount > MaxAttempts)
            {
                string ipBlockKey = $"otp:block:{recipient}:{clientIp}:{type}";
                await _cache.Database.StringSetAsync(
                    ipBlockKey,
                    "blocked",
                    TimeSpan.FromMinutes(LockoutMinutes)
                );
                _logger.Warning(
                    "IP blocked due to too many failed attempts for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                    recipient,
                    clientIp,
                    type
                );
                return new IpBlockedError(
                    "Too many attempts",
                    $"Too many failed verification attempts for this {type} and IP. IP temporarily blocked."
                );
            }
            return null;
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(
                _otpOption?.DomainUrl
                    ?? throw new InvalidOperationException("OTP service address not configured.")
            );
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Authorization",
                _otpOption?.ApiKey
            );
        }

        private async Task<(string?, ErrorDetails?)> GenerateAndSendOtpAsync(
            CreatePinRequest request,
            CancellationToken cancellationToken
        )
        {
            string code = GenerateOtpCode();

            if (request.Type == OtpType.Phone)
            {
                return await SendSmsOtpAsync(request, code, cancellationToken);
            }
            else
            {
                return await SendEmailOtpAsync(request, code, cancellationToken);
            }
        }

        private async Task<(string?, ErrorDetails?)> SendSmsOtpAsync(
            CreatePinRequest request,
            string code,
            CancellationToken cancellationToken
        )
        {
            var payload = new Dictionary<string, string>
            {
                { "to", request.To },
                { "message", $"[VietWash] Your verification code is: {code}" },
            };

            _logger.Information("Sending SMS OTP: to={Recipient}, code={Code}", request.To, code);

            try
            {
                using var content = ToJsonContent(payload);
                var response = await _httpClient.PostAsync("", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error("Failed to send SMS OTP: {Status}", response.StatusCode);
                    return (
                        null,
                        new ServiceError(
                            "SMS service error",
                            $"Failed to send SMS OTP: {response.Content}"
                        )
                    );
                }

                return (code, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Error sending OTP for phone: {Phone}", request.To);
                throw;
            }
        }

        private async Task<(string?, ErrorDetails?)> SendEmailOtpAsync(
            CreatePinRequest request,
            string code,
            CancellationToken cancellationToken
        )
        {
            _logger.Information("Sending Email OTP: to={Recipient}, code={Code}", request.To, code);

            try
            {
                var mailData = new MailTemplateData
                {
                    DisplayName = "VietWash",
                    Subject = "Your Verification Code",
                    To = new List<string> { request.To },
                    Template = new MailTemplate(
                        "OtpEmail",
                        new { Code = code, Expiry = OtpExpirationMinutes }
                    ),
                };

                var result = await _mailService.SendWithTemplateAsync(mailData);
                if (!result)
                {
                    _logger.Error("Failed to send Email OTP to: {Recipient}", request.To);
                    return (
                        null,
                        new ServiceError("Email service error", "Failed to send email OTP")
                    );
                }

                return (code, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error sending OTP for email: {Email}", request.To);
                throw;
            }
        }

        private async Task StoreOtpAsync(
            CreatePinRequest request,
            string otpCode,
            CancellationToken cancellationToken
        )
        {
            string key = $"otp:{request.To}:{request.ClientIp}:{request.Type}";

            await _cache.Database.StringGetAsync(key);

            var otpData = new
            {
                Code = otpCode,
                Ip = request.ClientIp,
                Type = request.Type.ToString(),
                ExpiresAt = DateTimeOffset
                    .UtcNow.AddMinutes(OtpExpirationMinutes)
                    .ToUnixTimeSeconds(),
            };

            await _cache.Database.StringSetAsync(
                key,
                JsonSerializer.Serialize(otpData),
                TimeSpan.FromMinutes(OtpExpirationMinutes)
            );

            _logger.Information(
                "Stored OTP for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                request.To,
                request.ClientIp,
                request.Type
            );
        }

        private async Task<bool> VerifyOtpAsync(
            VerifyPinRequest request,
            CancellationToken cancellationToken
        )
        {
            string key = $"otp:{request.To}:{request.ClientIp}:{request.Type}";
            var otpJson = await _cache.Database.StringGetAsync(key);
            if (!otpJson.HasValue)
            {
                _logger.Warning(
                    "No OTP found for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                    request.To,
                    request.ClientIp,
                    request.Type
                );
                return false;
            }

            var otpData = JsonSerializer.Deserialize<Dictionary<string, object>>(otpJson!);
            if (
                otpData == null
                || !otpData.TryGetValue("Code", out var code)
                || code?.ToString() != request.Otp!
            )
            {
                _logger.Warning(
                    "Invalid OTP for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                    request.To,
                    request.ClientIp,
                    request.Type
                );
                return false;
            }

            if (
                !otpData.TryGetValue("ExpiresAt", out var expiresAt)
                || !long.TryParse(expiresAt?.ToString(), out var expiresAtLong)
                || expiresAtLong < DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            )
            {
                _logger.Warning(
                    "OTP expired for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                    request.To,
                    request.ClientIp,
                    request.Type
                );
                return false;
            }

            if (
                !otpData.TryGetValue("Ip", out var storedIp)
                || storedIp?.ToString() != request.ClientIp
            )
            {
                _logger.Warning(
                    "IP mismatch for recipient: {Recipient}, stored: {StoredIp}, provided: {ProvidedIp}, type: {Type}",
                    request.To,
                    storedIp,
                    request.ClientIp,
                    request.Type
                );
                return false;
            }

            if (
                !otpData.TryGetValue("Type", out var storedType)
                || storedType?.ToString() != request.Type.ToString()
            )
            {
                _logger.Warning(
                    "Type mismatch for recipient: {Recipient}, stored: {StoredType}, provided: {ProvidedType}",
                    request.To,
                    storedType,
                    request.Type
                );
                return false;
            }

            await _cache.Database.StringGetAsync(key);
            _logger.Information(
                "OTP verified successfully for recipient: {Recipient}, ip: {Ip}, type: {Type}",
                request.To,
                request.ClientIp,
                request.Type
            );
            return true;
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
