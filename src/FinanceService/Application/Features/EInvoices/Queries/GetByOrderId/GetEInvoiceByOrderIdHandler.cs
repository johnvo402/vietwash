using Application.Common.Errors;
using Application.Common.Interfaces.Services.Aws;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Models.EInvoices;
using Application.Events.CreateEInvoiceEvents;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Pdf;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.EInvoices;
using Domain.Aggregates.EInvoices.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EInvoices.Queries.GetByOrderId
{
    public class GetEInvoiceByOrderIdHandler(IUnitOfWork unitOfWork,
        IPdfService pdfService,
        IMediaUpdateService media,
        IAmazonS3Service awsAmazonService
        )
        : IRequestHandler<GetEInvoiceByOrderIdQuery, Result<GetEInvoiceByOrderIdResponse>>
    {
        private const long CheckFileSize = 5 * 1024 * 1024;

        public async ValueTask<Result<GetEInvoiceByOrderIdResponse>> Handle(
            GetEInvoiceByOrderIdQuery request,
            CancellationToken cancellationToken
        )
        {
            var eInvoice = await unitOfWork
                .Repository<EInvoice>()
                .QueryAsync(x =>
                    x.OrderId == request.OrderId
                )
                .FirstOrDefaultAsync();

            if (eInvoice == null)
            {
                return Result<GetEInvoiceByOrderIdResponse>.Failure(
                    new NotFoundError(
                        "The source not found",
                        Messager
                            .Create<EInvoice>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            if(!string.IsNullOrEmpty(eInvoice.PdfUrl))
            {
                return Result<GetEInvoiceByOrderIdResponse>.Success(
               new GetEInvoiceByOrderIdResponse { Url = eInvoice.PdfUrl }
           );
            }
            var logo = awsAmazonService.GetFullpath(eInvoice.OrgLogo);
            var stamp = awsAmazonService.GetFullpath(eInvoice.OrgStamp);
            ReceiptModel result = eInvoice.MapToReceiptModel(logo ?? "", stamp ?? "");

            var pdfBytes = await pdfService.GeneratePdfAsync(
                new PdfGlobalParams { Template = new("Biennhan", result) }
            );
            if (pdfBytes is null || pdfBytes.Length == 0)
            {
                return Result<GetEInvoiceByOrderIdResponse>.Failure(
                    new NotFoundError(
                        "The source not found",
                        Messager
                            .Create<EInvoice>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
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
                return Result<GetEInvoiceByOrderIdResponse>.Failure(
                    new NotFoundError(
                        "The source not found",
                        Messager
                            .Create<EInvoice>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            eInvoice.Status = EInvoiceStatus.Published;
            eInvoice.PdfUrl = mediaKey;
            try
            {

                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Repository<EInvoice>().UpdateAsync(eInvoice);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result<GetEInvoiceByOrderIdResponse>.Success(
                new GetEInvoiceByOrderIdResponse { Url = mediaKey }
            );
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
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
