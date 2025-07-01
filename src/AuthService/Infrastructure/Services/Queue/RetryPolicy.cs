using Contracts.Dtos.Responses;
using Infrastructure.Services.DistributedCache;
using Serilog;

namespace Infrastructure.Services.Queue
{
    public class RetryPolicy
    {
        private readonly PubSubSettings _settings;
        private readonly ILogger _logger;
        private readonly bool _isDeadLetter;

        public RetryPolicy(PubSubSettings settings, ILogger logger, bool isDeadLetter = false)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isDeadLetter = isDeadLetter;
        }

        public async Task<TResponse> ExecuteAsync<TResponse>(
            Func<CancellationToken, Task<TResponse>> action,
            CancellationToken cancellationToken
        )
            where TResponse : class
        {
            int maxRetries = _isDeadLetter
                ? _settings.DeadLetterMaxRetryAttempts
                : _settings.MaxRetryAttempts;
            int attempt = 0;

            while (attempt <= maxRetries)
            {
                try
                {
                    var response = await action(cancellationToken);
                    if (response is PubSubResponse<object> pubSubResponse)
                    {
                        pubSubResponse.RetryCount = attempt;
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt > maxRetries)
                    {
                        _logger.Error(ex, "Request failed after {MaxRetries} retries", maxRetries);
                        throw;
                    }

                    double backoff = Math.Pow(
                        PubSubExtension.InitialSubscribeDelayInSeconds,
                        attempt
                    );
                    double jitter = PubSubExtension.GenerateJitter(
                        0,
                        PubSubExtension.MaximumJitterFactor
                    );
                    double delay = Math.Min(backoff + jitter, _settings.MaximumDelayInSec);

                    _logger.Warning(
                        "Retry {Attempt}/{MaxRetries} in {Delay:F2} seconds",
                        attempt,
                        maxRetries,
                        delay
                    );
                    await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
                }
            }

            throw new InvalidOperationException("Retry attempts exhausted");
        }
    }
}
