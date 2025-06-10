using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Inventories.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System.Data.Common;

namespace Application.Feature.InventoryImports.Command.Update
{
    public class UpdateInventoryImportHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper
    )
        : IRequestHandler<UpdateInventoryImportCommand, UpdateInventoryImportResponse>
    {
        public async ValueTask<UpdateInventoryImportResponse> Handle(UpdateInventoryImportCommand request, CancellationToken cancellationToken)
        {
            InventoryDocument? existingInventoryImport = await unitOfWork.Repository<InventoryDocument>().FindByConditionAsync(new GetInventoryDocumentByIdSpec(request.InventoryImportId), cancellationToken)
                        ?? throw new NotFoundException(
                 [Messager.Create<InventoryDocument>().Message(MessageType.Found).Negative().BuildMessage()]
             );
            decimal totalAmountBeforeDiscount = CalculateTotalBeforeDiscount(request);
            decimal totalAmountAfterDiscount = CalculateTotalAfterDiscount(request);
            existingInventoryImport.Amount = totalAmountBeforeDiscount;
            existingInventoryImport.Total = totalAmountAfterDiscount;

            if (request.Body.Status.HasValue)
            {
                if (request.Body.Status.Value < existingInventoryImport.Status)
                    throw new BadRequestException(
                        [Messager.Create<InventoryDocument>().Property(x => x.Status).Message(MessageType.Valid).Negative().Build()]);

                existingInventoryImport.Status = request.Body.Status.Value;
            }
            if (existingInventoryImport.Status == InventoryDocumentStatus.Completed)
            {
                existingInventoryImport.PaidAt = DateTimeOffset.UtcNow;
            }
            mapper.Map(request.Body.InventoryImportModel, existingInventoryImport);

            if (request.Body.InventoryImportModel.ProductItems != null)
                UpdateProductSupplyings(request, existingInventoryImport);

            if (request.Body.InventoryImportModel.EquipmentItems != null)
                UpdateEquipmentSupplyings(request, existingInventoryImport);


            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                await unitOfWork.Repository<InventoryDocument>().UpdateAsync(existingInventoryImport);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return mapper.Map<UpdateInventoryImportResponse>(existingInventoryImport);
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private decimal CalculateTotalBeforeDiscount(UpdateInventoryImportCommand command)
        {
            return command.Body.InventoryImportModel.ProductItems.Sum(item => item.Price * item.Quantity) +
                   command.Body.InventoryImportModel.EquipmentItems.Sum(item => item.Price);
        }

        private decimal CalculateTotalAfterDiscount(UpdateInventoryImportCommand command)
        {
            decimal totalAfterDiscount = 0;

            foreach (var item in command.Body.InventoryImportModel.ProductItems)
            {
                decimal discountAmount = CalculateDiscountAmount(item.Price, item.Discount);
                totalAfterDiscount += (item.Price - discountAmount) * item.Quantity;
            }

            foreach (var item in command.Body.InventoryImportModel.EquipmentItems)
            {
                decimal discountAmount = CalculateDiscountAmount(item.Price, item.Discount);
                totalAfterDiscount += (item.Price - discountAmount);
            }

            return totalAfterDiscount;
        }
        private decimal CalculateDiscountAmount(decimal price, decimal? discount)
        {
            if (!discount.HasValue) return 0;
            //Discount %
            if (discount.Value > 0 && discount.Value < 1)
            {
                return price * discount.Value;
            }
            else
            {
                return discount.Value <= price ? discount.Value : price;
            }
        }

        private void UpdateProductSupplyings(UpdateInventoryImportCommand request, InventoryDocument doc)
        {
            var existing = doc.ProductSupplyings.ToList();
            var updated = mapper.Map<List<ProductSupplying>>(request.Body.InventoryImportModel.ProductItems);

            foreach (var ps in updated)
            {
                var existingPs = existing.FirstOrDefault(eps => eps.Id == ps.Id); // Sử dụng Id để tìm bản ghi
                if (existingPs != null)
                {
                    // Cập bản hiện có
                    mapper.Map(ps, existingPs);
                }
                else
                {
                    // Thêm mới nếu không thấy
                    ps.LotNumber = $"LOT-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}";
                    ps.InventoryDocumentId = doc.Id;
                    existing.Add(ps);
                }
            }

            doc.ProductSupplyings = existing;
        }

        private void UpdateEquipmentSupplyings(UpdateInventoryImportCommand request, InventoryDocument doc)
        {
            var existing = doc.EquipmentSupplyings.ToList();
            var updated = mapper.Map<List<EquipmentSupplying>>(request.Body.InventoryImportModel.EquipmentItems);

            foreach (var es in updated)
            {
                var existingEs = existing.FirstOrDefault(ees => ees.Id == es.Id); // Sử dụng Id để tìm bản ghi
                if (existingEs != null)
                {
                    // Cập nhật bản hiện có
                    mapper.Map(es, existingEs);
                }
                else
                {
                    // Thêm mới nếu không tìm thấy
                    es.Code = $"COD-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}";
                    es.InventoryDocumentId = doc.Id;
                    existing.Add(es);
                }
            }

            doc.EquipmentSupplyings = existing;
        }


    }
}
