using Application.Common.Errors;
using Application.Common.Interfaces.Services.Aws;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Mapping.Inventories;
using Application.Feature.Common.Projections.Inventories;
using Application.Jobs;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Pdf;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.InventoryDocuments.Queries.GetReceipt
{
    public class InventoryReceiptHandler(
        IUnitOfWork unitOfWork,
        OrgSetting org,
        IPdfService pdfService,
        IMediaUpdateService media,
        IAmazonS3Service awsAmazonService
    ) : IRequestHandler<InventoryReceiptQuery, Result<InventoryReceiptResponse>>
    {
        private const long CheckFileSize = 5 * 1024 * 1024;

        public async ValueTask<Result<InventoryReceiptResponse>> Handle(
            InventoryReceiptQuery request,
            CancellationToken cancellationToken
        )
        {
            var inventory = await unitOfWork
                .Repository<InventoryDocument>()
                .QueryAsync(x => x.Status == InventoryStatus.Completed && x.Id == request.Id)
                .Select(x => new
                {
                    Inventory = x,
                    InventorySupplierReceipt = x
                        .InventorySupplierReceipts.Where(ps => ps.SupplierId == request.SupplierId)
                        .Select(ps => new { ps, ps.PdfUrl })
                        .FirstOrDefault(),
                    ProductSupplyings = x
                        .ProductSupplyings.Where(ps => ps.SupplierId == request.SupplierId)
                        .Select(ps => new
                        {
                            ps,
                            ps.Product,
                            ps.UnitRelation,
                            ps.Supplier,
                        })
                        .ToList(),
                    EquipmentSupplyings = x
                        .EquipmentSupplyings.Where(es => es.SupplierId == request.SupplierId)
                        .Select(es => new { es, es.Supplier })
                        .ToList(),
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (inventory == null)
            {
                return Result<InventoryReceiptResponse>.Failure(
                    new NotFoundError(
                        "Not found inventory",
                        Messager
                            .Create<InventoryDocument>()
                            .Message(MessageType.Found)
                            .Negative()
                            .Build()
                    )
                );
            }
            if (!string.IsNullOrEmpty(inventory.InventorySupplierReceipt?.PdfUrl))
            {
                return Result<InventoryReceiptResponse>.Success(
                    new() { Url = inventory.InventorySupplierReceipt.PdfUrl }
                );
            }
            string createdName = "--";
            if (long.TryParse(inventory.Inventory.CreatedBy, out var createdId))
            {
                var createdUser = await unitOfWork
                    .Repository<User>()
                    .FindByConditionAsync(
                        x => x.Id == createdId,
                        x => new OnlyId { Name = x.DisplayName },
                        cancellationToken: default
                    );
                if (createdUser?.Name != null)
                    createdName = createdUser.Name;
            }
            var branchName = await unitOfWork
                .Repository<BranchUser>()
                .FindByConditionAsync(
                    x => x.BranchId == inventory.Inventory.BranchId,
                    x => new OnlyId { Name = x.BranchName },
                    cancellationToken
                );

            InventoryReceiptModel invReceipt = inventory.Inventory.ToInventoryReceiptModel(
                createdName,
                branchName?.Name ?? "--"
            );
            invReceipt.OrgAddress = org.OrgAddress;
            invReceipt.OrgName = org.OrgName;
            var logo = awsAmazonService.GetFullpath(org.Logo);
            var stamp = awsAmazonService.GetFullpath(org.Stamp);
            if (!string.IsNullOrEmpty(logo))
                invReceipt.LogoUrl = logo;
            if (!string.IsNullOrEmpty(stamp))
                invReceipt.StampUrl = stamp;
            try
            {
                var pdfBytes = await pdfService.GeneratePdfAsync(
                    new PdfGlobalParams
                    {
                        Template = new(
                            inventory.Inventory.Type == InventoryType.Import
                                ? "InventoryImport"
                                : "InventoryExport",
                            invReceipt
                        ),
                    }
                );
                if (pdfBytes is null || pdfBytes.Length == 0)
                {
                    return Result<InventoryReceiptResponse>.Failure(
                        new BadRequestError(
                            "can gen pdf",
                            Messager
                                .Create<InventoryDocument>()
                                .Message(MessageType.Valid)
                                .Negative()
                                .Build()
                        )
                    );
                }
                var formFile = ConvertByteArrayToFormFile(pdfBytes, $"inv-{invReceipt.Code}.pdf");
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
                    return Result<InventoryReceiptResponse>.Failure(
                        new BadRequestError(
                            "can gen pdf",
                            Messager
                                .Create<InventoryDocument>()
                                .Message(MessageType.Valid)
                                .Negative()
                                .Build()
                        )
                    );
                }
                inventory.Inventory.InventorySupplierReceipts.Add(
                    new InventorySupplierReceipt
                    {
                        SupplierId = request.SupplierId,
                        PdfUrl = mediaKey,
                    }
                );
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                await unitOfWork.Repository<InventoryDocument>().UpdateAsync(inventory.Inventory);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result<InventoryReceiptResponse>.Success(new() { Url = mediaKey });
            }
            catch (System.Exception)
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
