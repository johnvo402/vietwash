using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Contracts.Infrastructure.Common;
using Contracts.Utils;
using Domain.Aggregates.EInvoices;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Events.CreateEInvoiceEvents
{
    public class CreateEInvoiceEventHandler(
        IUnitOfWork unitOfWork,
        OrgSetting org,
        IQrGenerator qrGenerator
    ) : IRequestHandler<CreateEInvoiceEvent, PubSubResponse<CreateEInvoiceEvent>>
    {
        public const string SourceEventIndexName = "ix_e_invoice_source_event_id";

        public async ValueTask<PubSubResponse<CreateEInvoiceEvent>> Handle(
            CreateEInvoiceEvent request,
            CancellationToken cancellationToken
        )
        {
            if (
                request.Payload is { } payload
                && await unitOfWork
                    .Repository<EInvoice>()
                    .AnyAsync(x => x.SourceEventId == request.PayloadId, cancellationToken)
            )
                return Success(request);

            var lookupCode = Generator.GenerateCode("HD", 6);
            var symbol = Generator.GenerateCode("C25T", 2);
            var qrCode = qrGenerator.GenerateQrBase64(lookupCode);
            var eInvoice = request.Payload?.CreateFromMessage(
                org: org,
                lookupCode: lookupCode,
                invoiceSymbol: symbol,
                qrCodeUrl: qrCode
            );
            if (eInvoice == null)
            {
                return new PubSubResponse<CreateEInvoiceEvent>
                {
                    Error = null,
                    ErrorType = PubSubErrorType.Transient,
                    IsSuccess = false,
                    ResponseData = request,
                    LastAttemptTime = DateTime.UtcNow,
                    PayloadId = request.PayloadId,
                };
            }
            eInvoice.SourceEventId = request.PayloadId;

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Repository<EInvoice>().AddAsync(eInvoice);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Success(request);
            }
            catch (DbUpdateException ex) when (IsDuplicateSourceEvent(ex))
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return Success(request);
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return new PubSubResponse<CreateEInvoiceEvent>
                {
                    Error = ex.Message,
                    ErrorType = PubSubErrorType.Persistent,
                    IsSuccess = false,
                    ResponseData = request,
                    LastAttemptTime = DateTime.UtcNow,
                    PayloadId = request.PayloadId,
                };
            }
        }

        public static bool IsDuplicateSourceEvent(DbUpdateException exception) =>
            exception.InnerException
                is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: SourceEventIndexName,
                };

        private static PubSubResponse<CreateEInvoiceEvent> Success(CreateEInvoiceEvent request) =>
            new()
            {
                Error = null,
                ErrorType = null,
                IsSuccess = true,
                ResponseData = request,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = request.PayloadId,
            };
    }
}
