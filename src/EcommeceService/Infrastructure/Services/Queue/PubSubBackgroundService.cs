using Application.Common.Interfaces.Services.DistributedCache;
using Application.Features.Users.Commands.Create;
using Contracts.Application.Common.Interfaces.Services.PubSub;
using Contracts.Dtos.Responses;
using Infrastructure.Services.Queue;
using JohnChum.SharedKernel.Extensions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectService_gRPC;
using Serilog;

namespace Infrastructure.Services.DistributedCache;

public class PubSubBackgroundService : BackgroundService
{
    private readonly IPubSubService _pubSubService;
    private readonly IServiceProvider _serviceProvider;
    private readonly PubSubSettings _pubSubSettings;
    private readonly ILogger _logger;

    public PubSubBackgroundService(
        IPubSubService pubSubService,
        IServiceProvider serviceProvider,
        IOptions<PubSubSettings> options,
        ILogger logger
    )
    {
        _pubSubService = pubSubService ?? throw new ArgumentNullException(nameof(pubSubService));
        _serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _pubSubSettings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check Redis connection
        try
        {
            bool isConnected = await _pubSubService.PingAsync();
            if (!isConnected)
            {
                _logger.Error("Failed to connect to Redis. PubSubBackgroundService cannot start.");
                throw new InvalidOperationException("Redis connection failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking Redis connection.");
            throw;
        }

        _logger.Information("PubSubBackgroundService started, subscribing to CreateAccountEvent.");

        // Subscribe to CreateAccountEvent
        _pubSubService.Subscribe<CreateAccountEvent>(async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var pubSubLogService = scope.ServiceProvider.GetRequiredService<IPubSubLogService>();
            var deadLetterPubSub = scope.ServiceProvider.GetRequiredService<IPubSubService>();

            var request = new CreateAccountCommand { Payload = message };

            await ProcessMessageAsync<CreateAccountCommand, PubSubResponse<CreateAccountCommand>>(
                request,
                sender,
                pubSubLogService,
                deadLetterPubSub,
                stoppingToken
            );
        });

        // Keep service running until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync<TRequest, TResponse>(
        TRequest request,
        ISender sender,
        IPubSubLogService pubSubLogService,
        IPubSubService deadLetterPubSub,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : PubSubResponse<TRequest>
    {
        var retryPolicy = new RetryPolicy(_pubSubSettings, _logger);
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
            _logger.Error(ex, "Unexpected error processing request.");
            await HandleFailedRequestAsync<TRequest, TResponse>(
                request,
                null,
                pubSubLogService,
                deadLetterPubSub,
                cancellationToken
            );
            return;
        }

        if (response.IsSuccess)
        {
            _logger.Information("Request {PayloadId} processed successfully", response.PayloadId);
            return;
        }

        await HandleFailedRequestAsync(
            request,
            response,
            pubSubLogService,
            deadLetterPubSub,
            cancellationToken
        );
    }

    private async Task HandleFailedRequestAsync<TRequest, TResponse>(
        TRequest request,
        TResponse? response,
        IPubSubLogService pubSubLogService,
        IPubSubService deadLetterPubSub,
        CancellationToken cancellationToken
    )
        where TRequest : class
        where TResponse : PubSubResponse<TRequest>
    {
        var requestId = response?.PayloadId?.ToString() ?? Guid.NewGuid().ToString();
        var requestData = SerializerExtension.Serialize(request).StringJson;
        var errorDetail =
            response?.Error != null
                ? SerializerExtension.Serialize(response.Error).StringJson
                : "Unexpected error during processing";

        var logRequest = new CreatePubSubLogRequest
        {
            RequestId = requestId,
            RequestData = requestData,
            ErrorDetail = errorDetail,
            ProcessedBy = PubSubType.OriginPubsub,
            RetryCount = response?.RetryCount ?? 0,
        };

        try
        {
            if (response?.ErrorType == PubSubErrorType.Persistent)
            {
                _logger.Error(
                    "Persistent error for request {PayloadId}: {Error}",
                    requestId,
                    errorDetail
                );
                await pubSubLogService.CreateLogAsync(logRequest, cancellationToken);
                return;
            }

            // Handle transient errors or unexpected failures
            _logger.Warning(
                "Pushing request {PayloadId} to dead letter queue after max retries or unexpected failure",
                requestId
            );
            logRequest.ErrorDetail = SerializerExtension
                .Serialize(
                    new
                    {
                        ErrorType = response?.ErrorType.ToString() ?? "Unknown",
                        Error = response?.Error ?? "Unexpected failure",
                        Message = $"Request {requestId} failed and is being pushed to dead letter queue",
                    }
                )
                .StringJson;

            var pushed = await deadLetterPubSub.PublishAsync(request);
            if (pushed)
            {
                _logger.Information(
                    "Request {PayloadId} successfully pushed to dead letter queue",
                    requestId
                );
            }
            else
            {
                _logger.Error("Failed to push request {PayloadId} to dead letter queue", requestId);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling failed request {PayloadId}", requestId);
        }

        try
        {
            var logResponse = await pubSubLogService.CreateLogAsync(logRequest, cancellationToken);
            if (!logResponse)
            {
                _logger.Error("Failed to log request {PayloadId} to PubSubLogService", requestId);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error logging request {PayloadId} to PubSubLogService", requestId);
        }
    }
}
