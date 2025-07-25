using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Aws;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Models.EInvoices;
using Contracts.Application.Common.Interfaces.Services.Pdf;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Infrastructure.Common;
using Contracts.Utils;
using Domain.Aggregates.EInvoices;
using Domain.Aggregates.EInvoices.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Events.CreateEInvoiceEvents
{
    public class CreateEInvoiceEventHandler(
        IUnitOfWork unitOfWork,
        OrgSetting org,
        IQrGenerator qrGenerator,
        IPdfService pdfService,
        IMediaUpdateService media,
        IAmazonS3Service awsAmazonService
    ) : IRequestHandler<CreateEInvoiceEvent, PubSubResponse<CreateEInvoiceEvent>>
    {
        private const long CheckFileSize = 5 * 1024 * 1024;

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
                var response = await unitOfWork.Repository<EInvoice>().AddAsync(eInvoice);
                await unitOfWork.SaveAsync(cancellationToken);

                var logo = awsAmazonService.GetFullpath(response.OrgLogo);
                var stamp = awsAmazonService.GetFullpath(response.OrgStamp);
                ReceiptModel result = response.MapToReceiptModel(logo ?? "", stamp ?? "");

                var pdfBytes = await pdfService.GeneratePdfAsync(
                    new PdfGlobalParams { Template = new("Biennhan", result) }
                );
                if (pdfBytes is null || pdfBytes.Length == 0)
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
                var formFile = ConvertByteArrayToFormFile(
                    pdfBytes,
                    $"hddt-{result.LookupCode}.pdf"
                );
                var mediaKey = media.GetKey(formFile, MediaType.File);
                if (formFile.Length >= CheckFileSize)
                {
                    await media.UploadMultiPartMediaAsync(formFile, mediaKey);
                }
                else
                {
                    await media.UploadMediaAsync(formFile, mediaKey);
                }
                if (mediaKey is null)
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
                response.Status = EInvoiceStatus.Published;
                response.PdfUrl = mediaKey;

                await unitOfWork.Repository<EInvoice>().UpdateAsync(response);
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

        private static IFormFile ConvertByteArrayToFormFile(
            byte[] fileBytes,
            string fileName,
            string contentType = "application/pdf"
        )
        {
            var stream = new MemoryStream(fileBytes);
            return new FormFile(stream, 0, fileBytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType,
            };
        }
    }
}
