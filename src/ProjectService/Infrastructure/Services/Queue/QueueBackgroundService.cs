using Application.Common.Interfaces.Services.DistributedCache;
using Application.Features.PubSubLogs;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Infrastructure.Services.DistributedCache;

public class PubSubBackgroundService(
    IPubSubFactory queueFactory,
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

        List<Task> runningTasks = new();

        while (!stoppingToken.IsCancellationRequested)
        {
            bool hasWork = false;

            for (int i = 0; i < 5; i++)
            {
                // PayCartPayload? request = await queueFactory
                //     .GetPubSub(PubSubType.Origin)
                //     .DequeueAsync<PayCartPayload, PayCartRequest>();

                // if (request != null)
                // {
                //     hasWork = true;
                //     var task = Task.Run(() =>
                //         ProcessWithRetryAsync<PayCartPayload, PayCartResponse>(
                //             request, sender, logger, stoppingToken), stoppingToken);
                //     runningTasks.Add(task);
                // }
            }

            if (!hasWork)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            runningTasks.RemoveAll(t => t.IsCompleted || t.IsFaulted || t.IsCanceled);
        }

        await Task.WhenAll(runningTasks);
    }

    private async Task ProcessWithRetryAsync<TRequest, TResponse>(
        TRequest request,
        ISender sender,
        ILogger logger,
        IPubSubService queueService,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : class
    {
        PubSubResponse<TResponse>? queueResponse = new();
        int attempt = 0;
        int maximumRetryAttempt = queueSettings.MaxRetryAttempts;
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
                CreatePubSubLogCommand createPubSubLogCommand = new()
                {
                    RequestId = queueResponse.PayloadId!.Value,
                    ErrorDetail = queueResponse.Error,
                    Request = request,
                    RetryCount = attempt,
                };
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
                double jitter = PubSubExtension.GenerateJitter(
                    0,
                    PubSubExtension.MaximumJitterFactor
                ); // Add jitter
                double delay = Math.Min(backoff + jitter, maximumDelay);

                TimeSpan delayTime = TimeSpan.FromSeconds(delay);
                logger.Warning($"Retry {attempt} in {delayTime.TotalSeconds:F2} seconds...");
                await Task.Delay(delayTime, cancellationToken);
            }
        }

        if (!queueResponse.IsSuccess && queueResponse.ErrorType == PubSubErrorType.Transient)
        {
            logger.Warning(
                "Push request {payloadId} into dead letter queue for maximum attempts",
                queueResponse.PayloadId
            );
            await queueService.PublishAsync(request);
            await sender.Send(
                new CreatePubSubLogCommand()
                {
                    RequestId = queueResponse.PayloadId!.Value,
                    ErrorDetail = new
                    {
                        queueResponse.ErrorType,
                        queueResponse.Error,
                        Message = $"Push request {queueResponse.PayloadId} into dead letter queue for maximum attempts",
                    },
                    Request = request,
                    RetryCount = queueResponse.RetryCount,
                },
                cancellationToken
            );
        }
    }
}
