using Application.Features.PubSubLogs;
using Grpc.Core;
using JohnChum.SharedKernel.Extensions;
using Mediator;
using ProjectService_gRPC;
using Serilog;

namespace Presentation.Services.gRPC.PubSubLog
{
    public class PubSubLogServiceHandler : PubSubLogService.PubSubLogServiceBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PubSubLogServiceHandler(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task<CreatePubSubLogResponse> CreatePubSubLog(
            CreatePubSubLogRequest request,
            ServerCallContext context
        )
        {
            try
            {
                // Kiểm tra RequestId hợp lệ
                if (!Guid.TryParse(request.RequestId, out var requestId))
                {
                    _logger.Warning("Invalid RequestId: {RequestId}", request.RequestId);
                    return new CreatePubSubLogResponse { Success = false };
                }

                // Kiểm tra deserialize dữ liệu
                object? requestData = null,
                    errorDetail = null;
                try
                {
                    requestData = SerializerExtension.Deserialize<object>(request.RequestData);
                    errorDetail = SerializerExtension.Deserialize<object>(request.ErrorDetail);
                }
                catch (Exception ex)
                {
                    _logger.Warning(
                        "Failed to deserialize request data. Error: {Message}",
                        ex.Message
                    );
                    return new CreatePubSubLogResponse { Success = false };
                }

                var command = new CreatePubSubLogCommand
                {
                    RequestId = requestId,
                    Request = requestData,
                    ErrorDetail = errorDetail,
                    ProcessedBy = (Domain.Aggregates.PubSubLogs.PubSubType)request.ProcessedBy,
                    RetryCount = request.RetryCount,
                };

                await _mediator.Send(command);
                return new CreatePubSubLogResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed queue log.");
                return new CreatePubSubLogResponse { Success = false };
            }
        }
    }
}
