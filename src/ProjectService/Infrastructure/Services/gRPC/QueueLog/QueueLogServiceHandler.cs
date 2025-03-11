using Application.Features.QueueLogs;
using Grpc.Core;
using JohnChum.SharedKernel.Extensions;
using Mediator;
using ProjectService_gRPC;
using Serilog;

namespace Presentation.Services.gRPC.QueueLog
{
    public class QueueLogServiceHandler : QueueLogService.QueueLogServiceBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public QueueLogServiceHandler(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task<CreateQueueLogResponse> CreateQueueLog(CreateQueueLogRequest request, ServerCallContext context)
        {
            try
            {
                // Kiểm tra RequestId hợp lệ
                if (!Guid.TryParse(request.RequestId, out var requestId))
                {
                    _logger.Warning("Invalid RequestId: {RequestId}", request.RequestId);
                    return new CreateQueueLogResponse { Success = false };
                }

                // Kiểm tra deserialize dữ liệu
                object? requestData = null, errorDetail = null;
                try
                {
                    requestData = SerializerExtension.Deserialize<object>(request.RequestData);
                    errorDetail = SerializerExtension.Deserialize<object>(request.ErrorDetail);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Failed to deserialize request data. Error: {Message}", ex.Message);
                    return new CreateQueueLogResponse { Success = false };
                }

                var command = new CreateQueueLogCommand
                {
                    RequestId = requestId,
                    Request = requestData,
                    ErrorDetail = errorDetail,
                    ProcessedBy = (Domain.Aggregates.QueueLogs.QueueType)request.ProcessedBy,
                    RetryCount = request.RetryCount
                };

                await _mediator.Send(command);
                return new CreateQueueLogResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed queue log.");
                return new CreateQueueLogResponse { Success = false };
            }
        }
    }
}
