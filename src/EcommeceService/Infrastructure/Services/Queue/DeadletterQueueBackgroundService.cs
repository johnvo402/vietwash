using Application.Common.Interfaces.Services.DistributedCache;
using Application.Features.Users.Commands.Create;
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

public class DeadletterQueueBackgroundService(
    IQueueFactory factory,
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
            IQueueService originQueue = factory.GetQueue(Domain.Aggregates.QueueLogs.QueueType.DeadLetterQueue);

            if (!await originQueue.PingAsync())
            {
                logger.Warning("Redis server has shut down");
                continue;
            }
            bool hasWork = false;

            for (int i = 0; i < 5; i++)
            {
                CreateUserCommand? request = await originQueue
                    .DequeueAsync<CreateUserCommand, CreateUserCommand>();

                if (request != null)
                {
                    hasWork = true;
                    var task = Task.Run(async () =>
                    {
                        using var taskScope = serviceProvider.CreateScope();
                        var taskSender = taskScope.ServiceProvider.GetRequiredService<ISender>();
                        var taskLogger = taskScope.ServiceProvider.GetRequiredService<ILogger>();
                        var taskGrpcClient = taskScope.ServiceProvider.GetRequiredService<IQueueLogService>();


                        await ProcessWithRetryAsync<CreateUserCommand, CreateUserCommand>(
                            request, taskSender, taskGrpcClient, taskLogger, originQueue, stoppingToken);
                    }, stoppingToken);
                    runningTasks.Add(task);
                }
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
        int maximumRetryAttempt = queueSettings.DeadLetterMaxRetryAttempts;
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
                var createQueueLogCommand = MaptoCreateQueueLogCommand(
                    queueResponse,
                    request
                );
                await _grpcClient.CreateLogAsync(createQueueLogCommand, cancellationToken);
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
                logger.Warning(
                    $"Dead letter queue Retry {attempt} in {delayTime.TotalSeconds:F2} seconds..."
                );
                await Task.Delay(delayTime, cancellationToken);
            }
        }

        if (!queueResponse.IsSuccess && queueResponse.ErrorType == QueueErrorType.Transient)
        {
            // if it still fail after many attempts then logging into db
            var createQueueLogCommand = MaptoCreateQueueLogCommand(
                queueResponse,
                request
            );
            await _grpcClient.CreateLogAsync(createQueueLogCommand, cancellationToken);
        }
    }

    private static CreateQueueLogRequest MaptoCreateQueueLogCommand<TResponse, TRequest>(
        QueueResponse<TResponse> response,
        TRequest request
    )
        where TRequest : class
        where TResponse : class
    {
        return new CreateQueueLogRequest()
        {
            RequestId = response.PayloadId!.Value.ToString(),
            ErrorDetail = response.Error?.ToString(),
            RequestData = request.ToString(),
            RetryCount = response.RetryCount,
            ProcessedBy = ProjectService_gRPC.QueueType.DeadLetterQueue,
        };
    }
}
