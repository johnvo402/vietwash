using Application.Common.Interfaces.Services.DistributedCache;
using Application.Features.PubSubLogs;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Infrastructure.Services.DistributedCache;

public class DeadletterPubSubBackgroundService(
    IPubSubFactory factory,
    IServiceProvider serviceProvider,
    IOptions<PubSubSettings> options
) : BackgroundService
{
    private readonly PubSubSettings queueSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        while (!stoppingToken.IsCancellationRequested)
        {
            // IPubSubService deadLetterPubSub = factory.GetPubSub(PubSubType.DeadLetter);

            // if (!await deadLetterPubSub.PingAsync())
            // {
            //     logger.Warning("Redis server has shut down");
            //     continue;
            // }
            // var request = await deadLetterPubSub.DequeueAsync<PayCartPayload, PayCartPayload>();

            // if (request != null)
            // {
            //     await ProcessWithRetryAsync<PayCartPayload, PayCartResponse>(
            //         request,
            //         sender,
            //         logger,
            //         stoppingToken
            //     );
            // }
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private async Task ProcessWithRetryAsync<TRequest, TResponse>(
        TRequest request,
        ISender sender,
        ILogger logger,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : class
    {
        PubSubResponse<TResponse>? queueResponse = new();
        int attempt = 0;
        int maximumRetryAttempt = queueSettings.DeadLetterMaxRetryAttempts;
        double maximumDelay = queueSettings.MaximumDelayInSec;

        while (attempt <= maximumRetryAttempt)
        {
            queueResponse =
                await sender.Send(request, cancellationToken) as PubSubResponse<TResponse>;

            // sucess case
            if (queueResponse!.IsSuccess)
            {
                logger.Information(
                    "excuting request {payloadId} has been success!",
                    queueResponse.PayloadId
                );
                break;
            }

            // 500 or 400 error
            if (queueResponse.ErrorType == PubSubErrorType.Persistent)
            {
                CreatePubSubLogCommand createPubSubLogCommand = MaptoCreatePubSubLogCommand(
                    queueResponse,
                    request
                );
                await sender.Send(createPubSubLogCommand, cancellationToken);
                break;
            }

            // transient error retry but
            if (queueResponse.ErrorType == PubSubErrorType.Transient)
            {
                attempt++;
                if (attempt > maximumRetryAttempt)
                {
                    break;
                }
                queueResponse.RetryCount = attempt;

                // Calculate delay time with exponential jitter backoff method
                // 1st -> 2.1s; 2nd -> 4.2; 3rd -> 8.2; 4th -> 16.1
                double backoff = Math.Pow(PubSubExtension.InitialSubscribeDelayInSeconds, attempt); // Exponential backoff (2^attempt)
                double jitter = PubSubExtension.GenerateJitter(0, PubSubExtension.MaximumJitterFactor); // Add jitter
                double delay = Math.Min(backoff + jitter, maximumDelay);

                TimeSpan delayTime = TimeSpan.FromSeconds(delay);
                logger.Warning(
                    $"Dead letter queue Retry {attempt} in {delayTime.TotalSeconds:F2} seconds..."
                );
                await Task.Delay(delayTime, cancellationToken);
            }
        }

        if (!queueResponse.IsSuccess && queueResponse.ErrorType == PubSubErrorType.Transient)
        {
            // if it still fail after many attempts then logging into db
            CreatePubSubLogCommand createPubSubLogCommand = MaptoCreatePubSubLogCommand(
                queueResponse,
                request
            );
            await sender.Send(createPubSubLogCommand, cancellationToken);
        }
    }

    private static CreatePubSubLogCommand MaptoCreatePubSubLogCommand<TResponse, TRequest>(
        PubSubResponse<TResponse> response,
        TRequest request
    )
        where TRequest : class
        where TResponse : class
    {
        return new CreatePubSubLogCommand()
        {
            RequestId = response.PayloadId!.Value,
            ErrorDetail = response.Error,
            Request = request,
            RetryCount = response.RetryCount,
            ProcessedBy = Domain.Aggregates.PubSubLogs.PubSubType.DeadLetter,
        };
    }
}
