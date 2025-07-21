using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Queries.Detail;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Pdf;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Feature.Orders.Queries.GetReceipt;

public class GetReceiptHandler(
    IPdfService pdfService,
    IMediaUpdateService media,
    IUnitOfWork unitOfWork
) : IRequestHandler<GetReceiptQuery, Result<GetReceiptResponse>>
{
    public async ValueTask<Result<GetReceiptResponse>> Handle(
        GetReceiptQuery request,
        CancellationToken cancellationToken
    )
    {
        // Step 1: Get order details
        var orderDetailResult = await unitOfWork
            .DynamicReadOnlyRepository<Order>()
            .FindByConditionAsync(
                new GetOrderByIdSpecification(request.OrderId),
                cancellationToken
            );

        if (orderDetailResult is null)
        {
            return Result<GetReceiptResponse>.Failure(
                new NotFoundError(
                    "Order not found",
                    Messager.Create<Order>().Message(MessageType.Found).Negative().Build()
                )
            );
        }
        var receipt = orderDetailResult.Receipt;
        if (!string.IsNullOrEmpty(receipt))
        {
            return Result<GetReceiptResponse>.Success(
                new GetReceiptResponse { ReceiptUrl = receipt }
            );
        }
        // Step 2: Generate PDF
        var pdfBytes = await pdfService.GeneratePdfAsync(
            new PdfGlobalParams { Template = new("Biennhan", orderDetailResult) }
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
        try
        {
            orderDetailResult.Receipt = mediaKey;
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Repository<Order>().UpdateAsync(orderDetailResult);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        var fileUrl = await media.UploadMediaAsync(formFile, mediaKey);

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
