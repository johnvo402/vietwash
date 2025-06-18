using Application.Common.Interfaces.Services.DistributedCache;
using Application.Features.Users.Commands.Create;
using Contracts.Application.Common.Interfaces.Services.PubSub;
using Contracts.Dtos.Responses;
using Domain.Aggregates.PubSubLogs;
using Infrastructure.Services.Queue;
using JohnChum.SharedKernel.Extensions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectService_gRPC;
using Serilog;

namespace Infrastructure.Services.DistributedCache;

public class DeadletterPubSubBackgroundService : BackgroundService
{
    private readonly IPubSubFactory _factory;
    private readonly IServiceProvider _serviceProvider;
    private readonly PubSubSettings _settings;
    private readonly ILogger _logger;
    private readonly List<Task> _runningTasks;

    public DeadletterPubSubBackgroundService(
        IPubSubFactory factory,
        IServiceProvider serviceProvider,
        IOptions<PubSubSettings> options,
        ILogger logger
    )
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _runningTasks = new List<Task>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pubSubService = _factory.GetPubSub(Domain.Aggregates.PubSubLogs.PubSubType.DeadLetter);
        try
        {
            if (!await pubSubService.PingAsync())
            {
                _logger.Error(
                    "Redis connection failed for dead-letter queue. Service cannot start."
                );
                throw new InvalidOperationException("Redis connection failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking Redis connection for dead-letter queue.");
            throw;
        }

        _logger.Information(
            "DeadletterPubSubBackgroundService started, subscribing to dead-letter queue."
        );

        pubSubService.Subscribe<CreateAccountEvent>(async message =>
        {
            // Limit concurrent tasks
            if (_runningTasks.Count >= _settings.DeadLetterMaxRetryAttempts)
            {
                _logger.Warning(
                    "Max concurrent tasks reached ({MaxTasks}). Waiting for tasks to complete.",
                    _settings.DeadLetterMaxRetryAttempts
                );
                await Task.WhenAny(_runningTasks);
                _runningTasks.RemoveAll(t => t.IsCompleted || t.IsFaulted || t.IsCanceled);
            }

            var task = Task.Run(
                async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                    var pubSubLogService =
                        scope.ServiceProvider.GetRequiredService<IPubSubLogService>();
                    var request = new CreateAccountCommand { Payload = message };
                    await ProcessMessageAsync<
                        CreateAccountCommand,
                        PubSubResponse<CreateAccountCommand>
                    >(request, sender, pubSubLogService, logger, stoppingToken);
                },
                stoppingToken
            );

            lock (_runningTasks)
            {
                _runningTasks.Add(task);
            }

            // Clean up completed tasks
            try
            {
                await Task.WhenAny(_runningTasks);
                lock (_runningTasks)
                {
                    foreach (var failedTask in _runningTasks.FindAll(t => t.IsFaulted))
                    {
                        _logger.Error(
                            failedTask.Exception,
                            "Task processing dead-letter message failed."
                        );
                    }
                    _runningTasks.RemoveAll(t => t.IsCompleted || t.IsFaulted || t.IsCanceled);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error cleaning up tasks.");
            }
        });

        // Keep service running until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync<TRequest, TResponse>(
        TRequest request,
        ISender sender,
        IPubSubLogService pubSubLogService,
        ILogger logger,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : PubSubResponse<TRequest>
    {
        var retryPolicy = new RetryPolicy(_settings, logger, isDeadLetter: true);
        TResponse response;

        try
        {
            response = await retryPolicy.ExecuteAsync(
                async (ct) =>
                {
                    var result = await sender.Send(request, ct);
                    if (result is not TResponse typed)
                        throw new InvalidOperationException(
                            $"Invalid response type. Got: {result?.GetType().FullName}"
                        );

                    return typed;
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error processing dead-letter request.");
            await LogFailureAsync<TRequest, TResponse>(
                request,
                null,
                pubSubLogService,
                logger,
                cancellationToken
            );
            return;
        }

        if (response?.IsSuccess == true)
        {
            logger.Information(
                "Dead-letter request {PayloadId} processed successfully",
                response.PayloadId
            );
            return;
        }

        await LogFailureAsync(request, response, pubSubLogService, logger, cancellationToken);
    }

    private async Task LogFailureAsync<TRequest, TResponse>(
        TRequest request,
        TResponse? response,
        IPubSubLogService pubSubLogService,
        ILogger logger,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : PubSubResponse<TRequest>
    {
        var requestId = response?.PayloadId?.ToString() ?? Guid.NewGuid().ToString();
        var requestData = SerializerExtension.Serialize(request).StringJson;
        var errorDetail =
            response?.Error != null
                ? SerializerExtension
                    .Serialize(
                        new
                        {
                            ErrorType = response.ErrorType.ToString(),
                            response.Error,
                            Message = $"Dead-letter request {requestId} failed",
                        }
                    )
                    .StringJson
                : "Unexpected error during processing";

        var logRequest = new CreatePubSubLogRequest
        {
            RequestId = requestId,
            RequestData = requestData,
            ErrorDetail = errorDetail,
            ProcessedBy = ProjectService_gRPC.PubSubType.DeadLetterPubsub,
            RetryCount = response?.RetryCount ?? 0,
        };

        try
        {
            var logResponse = await pubSubLogService.CreateLogAsync(logRequest, cancellationToken);
            if (!logResponse)
            {
                logger.Error(
                    "Failed to log dead-letter request {RequestId} to PubSubLogService",
                    requestId
                );
            }
            else
            {
                logger.Information(
                    "Logged dead-letter request {RequestId} to PubSubLogService",
                    requestId
                );
            }
        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "Error logging dead-letter request {RequestId} to PubSubLogService",
                requestId
            );
        }
    }
}
