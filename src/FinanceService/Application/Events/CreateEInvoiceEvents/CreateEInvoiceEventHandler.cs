using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Contracts.Infrastructure.Common;
using Contracts.Utils;
using Domain.Aggregates.EInvoices;
using Mediator;

namespace Application.Events.CreateEInvoiceEvents
{
    public class CreateEInvoiceEventHandler(
        IUnitOfWork unitOfWork,
        OrgSetting org,
        IQrGenerator qrGenerator
    ) : IRequestHandler<CreateEInvoiceEvent, PubSubResponse<CreateEInvoiceEvent>>
    {

        public async ValueTask<PubSubResponse<CreateEInvoiceEvent>> Handle(
            CreateEInvoiceEvent request,
            CancellationToken cancellationToken
        )
        {
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

            try
            {

                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Repository<EInvoice>().AddAsync(eInvoice);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return new PubSubResponse<CreateEInvoiceEvent>
                {
                    Error = null,
                    ErrorType = null,
                    IsSuccess = true,
                    ResponseData = request,
                    LastAttemptTime = DateTime.UtcNow,
                    PayloadId = request.PayloadId,
                };
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

    }
}
