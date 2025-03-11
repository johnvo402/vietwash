using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.Application.Common.Interfaces.Services.Queue;
using Contracts.Dtos.Responses;
using Domain.Aggregates.QueueLogs;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectService_gRPC;
using Serilog;

namespace Infrastructure.Services.DistributedCache;

public class QueueBackgroundService(
    IQueueFactory queueFactory,
    IServiceProvider serviceProvider,
    IOptions<QueueSettings> options
) : BackgroundService
{
    private readonly QueueSettings queueSettings = options.Value;

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
                //     .GetQueue(QueueType.OriginQueue)
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
        IQueueLogService _grpcClient,
        ILogger logger,
        IQueueService queueService,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : class
    {
        QueueResponse<TResponse>? queueResponse = new();
        int attempt = 0;
        int maximumRetryAttempt = queueSettings.MaxRetryAttempts;
        double maximumDelay = queueSettings.MaximumDelayInSec;

        while (attempt <= maximumRetryAttempt)
        {
            queueResponse =
                await sender.Send(request, cancellationToken) as QueueResponse<TResponse>;

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
            if (queueResponse.ErrorType == QueueErrorType.Persistent)
            {
                var requestLog = new CreateQueueLogRequest
                {
                    RequestId = queueResponse.PayloadId!.Value.ToString(),
                    RequestData = request.ToString(),
                    ErrorDetail = queueResponse.Error?.ToString(),
                    ProcessedBy = ProjectService_gRPC.QueueType.OriginQueue,
                    RetryCount = attempt,
                };
                await _grpcClient.CreateLogAsync(requestLog, cancellationToken);
                break;
            }

            // transient error retry but
            if (queueResponse.ErrorType == QueueErrorType.Transient)
            {
                attempt++;
                if (attempt > maximumRetryAttempt)
                {
                    break;
                }

                queueResponse.RetryCount = attempt;

                // Calculate delay time with exponential jitter backoff method
                // 1st -> 2.1s; 2nd -> 4.2; 3rd -> 8.2; 4th -> 16.1
                double backoff = Math.Pow(QueueExtention.INIT_DELAY, attempt); // Exponential backoff (2^attempt)
                double jitter = QueueExtention.GenerateJitter(0, QueueExtention.MAXIMUM_JITTER); // Add jitter
                double delay = Math.Min(backoff + jitter, maximumDelay);

                TimeSpan delayTime = TimeSpan.FromSeconds(delay);
                logger.Warning($"Retry {attempt} in {delayTime.TotalSeconds:F2} seconds...");
                await Task.Delay(delayTime, cancellationToken);
            }
        }

        if (!queueResponse.IsSuccess && queueResponse.ErrorType == QueueErrorType.Transient)
        {
            // if it still fail after many attempts then push it into dead letter queue
            logger.Warning(
                "Push request {payloadId} into dead letter queue for maximum attempts",
                queueResponse.PayloadId
            );
            var requestLog = new CreateQueueLogRequest
            {
                RequestId = queueResponse.PayloadId!.Value.ToString(),
                RequestData = request.ToString(),
                ErrorDetail = new
                {
                    queueResponse.ErrorType,
                    queueResponse.Error,
                    Message = $"Push request {queueResponse.PayloadId} into dead letter queue for maximum attempts",
                }.ToString(),
                ProcessedBy = ProjectService_gRPC.QueueType.OriginQueue,
                RetryCount = queueResponse.RetryCount
            };
   
            await queueService.EnqueueAsync(request);
            await _grpcClient.CreateLogAsync(requestLog, cancellationToken);

        }
    }

}
