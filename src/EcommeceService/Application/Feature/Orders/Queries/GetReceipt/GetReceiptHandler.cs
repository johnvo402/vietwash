using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Feature.Orders.Queries.Detail;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Pdf;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.Orders;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Feature.Orders.Queries.GetReceipt;

public class GetReceiptHandler(ISender sender, IPdfService pdfService, IMediaUpdateService media)
    : IRequestHandler<GetReceiptQuery, Result<GetReceiptResponse>>
{
    public async ValueTask<Result<GetReceiptResponse>> Handle(
        GetReceiptQuery request,
        CancellationToken cancellationToken
    )
    {
        // Step 1: Get order details
        var orderDetailResult = await sender.Send(
            new GetOrderDetailQuery { OrderId = request.OrderId },
            cancellationToken
        );

        if (!orderDetailResult.IsSuccess || orderDetailResult.Value is null)
        {
            return Result<GetReceiptResponse>.Failure(
                orderDetailResult.Error
                    ?? new NotFoundError(
                        "Order not found",
                        Messager.Create<Order>().Message(MessageType.Found).Negative().Build()
                    )
            );
        }

        // Step 2: Generate PDF
        var pdfBytes = await pdfService.GeneratePdfAsync(
            new PdfGlobalParams { Template = new("Biennhan", orderDetailResult.Value) }
        );

        if (pdfBytes is null || pdfBytes.Length == 0)
        {
            return Result<GetReceiptResponse>.Failure(
                new BadRequestError(
                    "Failed to generate receipt PDF",
                    Messager.Create<Order>().Message(MessageType.Empty).Build()
                )
            );
        }

        // Step 3: Convert PDF to FormFile
        var formFile = ConvertByteArrayToFormFile(pdfBytes, $"bien-nhan-{request.OrderId}.pdf");

        // Step 4: Upload File
        var mediaKey = media.GetKey(formFile, MediaType.File);

        if (mediaKey is null)
        {
            return Result<GetReceiptResponse>.Failure(
                new BadRequestError(
                    "Media key generation failed",
                    Messager.Create<Order>().Message(MessageType.Empty).Build()
                )
            );
        }

        var fileUrl = await media.UploadAvatarAsync(formFile, mediaKey);

        if (string.IsNullOrEmpty(fileUrl))
        {
            return Result<GetReceiptResponse>.Failure(
                new BadRequestError(
                    "Failed to upload receipt",
                    Messager.Create<Order>().Message(MessageType.Empty).Build()
                )
            );
        }

        return Result<GetReceiptResponse>.Success(new GetReceiptResponse { ReceiptUrl = fileUrl });
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
