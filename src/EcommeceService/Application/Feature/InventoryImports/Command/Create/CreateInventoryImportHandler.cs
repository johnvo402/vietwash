using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Services;
using Infrastructure.Services.Identity;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wangkanai.Extensions;

namespace Application.Feature.InventoryImports.Command.Create
{
    public class CreateInventoryImportHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<CreateInventoryImportCommand>
    {
        public async ValueTask<Mediator.Unit> Handle(
            CreateInventoryImportCommand command,
            CancellationToken cancellationToken)
        {
            decimal totalAmountBeforeDiscount = CalculateTotalBeforeDiscount(command);
            decimal totalAmountAfterDiscount = CalculateTotalAfterDiscount(command);

            var inventoryDocumentMapping = mapper.Map<InventoryDocument>(command);
            inventoryDocumentMapping.Amount = totalAmountBeforeDiscount;
            inventoryDocumentMapping.Total = totalAmountAfterDiscount;
            inventoryDocumentMapping.TransactionAt = DateTime.UtcNow;
            inventoryDocumentMapping.Code = $"INV-{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..]}";

            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(
                    cancellationToken
                );
                if (!inventoryDocumentMapping.ProductSupplyings.IsEmpty())
                {
                    foreach (var ps in inventoryDocumentMapping.ProductSupplyings)
                    {
                        ps.LotNumber = $"LOT-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}";
                        ps.ArriveAt = command.ArrivedAt ?? DateTimeOffset.UtcNow;
                        ps.InventoryDocumentId = inventoryDocumentMapping.Id;
                    }
                }
                if (!inventoryDocumentMapping.EquipmentSupplyings.IsEmpty())
                {
                    foreach (var es in inventoryDocumentMapping.EquipmentSupplyings)
                    {
                        es.Code = $"COD-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}";
                        es.ArrivedAt = command.ArrivedAt ?? DateTimeOffset.UtcNow;
                        es.InventoryDocumentId = inventoryDocumentMapping.Id;
                    }
                }

                var inventoryDocument = await unitOfWork
                    .Repository<InventoryDocument>()
                    .AddAsync(inventoryDocumentMapping, cancellationToken);

                //var productSupplyings = mapper.Map<List<ProductSupplying>>(command.ProductItems);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Mediator.Unit.Value;
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private decimal CalculateTotalBeforeDiscount(CreateInventoryImportCommand command)
        {
            return command.ProductItems.Sum(item => item.Price * item.Quantity) +
                   command.EquipmentItems.Sum(item => item.Price);
        }

        private decimal CalculateTotalAfterDiscount(CreateInventoryImportCommand command)
        {
            decimal totalAfterDiscount = 0;

            foreach (var item in command.ProductItems)
            {
                decimal discountAmount = CalculateDiscountAmount(item.Price, item.Discount);
                totalAfterDiscount += (item.Price - discountAmount) * item.Quantity;
            }

            foreach (var item in command.EquipmentItems)
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
    }
}
