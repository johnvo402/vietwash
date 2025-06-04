using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Otp;
using Microsoft.Extensions.Options;
using Serilog;
using System.Data.Common;
using System.Text;
using System.Text.Json;


namespace Infrastructure.Services.Identity
{
    public class SmsOtpClient : ISmsOtpClient
    {
        private readonly OtpOption _otpOption;
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentAccount _currentAccount;
        private static readonly Random _random = new Random();
        private readonly ILogger _logger;

        public SmsOtpClient(
            IOptions<OtpOption> otpOption,
            HttpClient httpClient,
            IUnitOfWork unitOfWork,
            ICurrentAccount currentAccount,
            ILogger logger)
        {
            _otpOption = otpOption?.Value ?? throw new ArgumentNullException(nameof(otpOption));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentAccount = currentAccount ?? throw new ArgumentNullException(nameof(currentAccount));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.BaseAddress = new Uri(_otpOption.DomainUrl!);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _otpOption.ApiKey);
        }

        public async Task CreatePinAsync(CreatePinRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.To)) throw new ArgumentException("Recipient phone number cannot be empty.", nameof(request.To));

            string code = GenerateOtpCode();
            var payload = new Dictionary<string, string>
            {
                { "to", request.To },
                { "message", $"[VietWash] Your verification code is: {code}" }
            };

            _logger.Information("Sending SMS OTP: to={To}, code={Code}", request.To, code);

            using var content = ToJsonContent(payload);
            DbTransaction? transaction = null;

            try
            {
                var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error("Failed to send SMS OTP: {ReasonPhrase}", response.ReasonPhrase);
                    throw new HttpRequestException($"Failed to send SMS OTP: {response.ReasonPhrase}");
                }

                transaction = await _unitOfWork.CreateTransactionAsync(cancellationToken);
                var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();

                await _unitOfWork.Repository<AccountActivity>().AddAsync(new AccountActivity
                {
                    AccountId = request.AccountId,
                    Type = request.Type,
                    Ip = _currentAccount.ClientIp,
                    Metadata = new Dictionary<string, string>
                    {
                        { "to", request.To },
                        { "code", code },
                        { "expiresAt", expiresAt.ToString() }
                    }
                }, cancellationToken);

                await _unitOfWork.SaveAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.Information("Successfully created and stored OTP for AccountId={AccountId}", request.AccountId);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                }
                _logger.Error(ex, "Error creating OTP for AccountId={AccountId}", request.AccountId);
                throw new Exception("Failed to create OTP.", ex);
            }
        }

        public async Task<bool> VerifyPinAsync(VerifyPinRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Otp)) throw new ArgumentException("OTP cannot be empty.", nameof(request.Otp));

            try
            {
                var accountActivity = await _unitOfWork.Repository<AccountActivity>()
                    .FindByConditionAsync(new GetAccountActivitySpecification(request.AccountId, request.Type), cancellationToken);

                if (accountActivity == null)
                {
                    _logger.Warning("No account activity found for AccountId={AccountId}, Type={Type}", request.AccountId, request.Type);
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

                if (!accountActivity.Metadata.TryGetValue("expiresAt", out var expiresAt) ||
                    !long.TryParse(expiresAt, out var expiresAtLong) ||
                    expiresAtLong < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    _logger.Warning("OTP expired for AccountId={AccountId}", request.AccountId);
                    return false;
                }

                _logger.Information("OTP verified successfully for AccountId={AccountId}", request.AccountId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error verifying OTP for AccountId={AccountId}", request.AccountId);
                throw new Exception("Failed to verify OTP.", ex);
            }
        }

        private static string GenerateOtpCode()
        {
            const int otpLength = 6;
            int maxValue = (int)Math.Pow(10, otpLength) - 1;
            int minValue = (int)Math.Pow(10, otpLength - 1);
            return _random.Next(minValue, maxValue + 1).ToString("D6");
        }

        private static StringContent ToJsonContent<T>(T data) =>
            new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
    }
}