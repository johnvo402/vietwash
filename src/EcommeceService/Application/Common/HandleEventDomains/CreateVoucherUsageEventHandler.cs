using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.Detail;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Events;
using Domain.Aggregates.Vouchers.Specifications;
using Domain.Events;
using Mediator;
using Serilog;

namespace Application.Common.HandleEventDomains
{
    public class CreateVoucherUsageEventHandler(IUnitOfWork unitOfWork, ILogger logger)
        : INotificationHandler<VoucherUsageEvent>
    {
        public async ValueTask Handle(VoucherUsageEvent notification, CancellationToken cancellationToken)
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                logger.Information("Handling VoucherUsageEvent for OrderId: {@OrderId}", notification.OrderId);

                var usage = new VoucherUsage(
                    voucherId: notification.VoucherId,
                    customerId: notification.CustomerId,
                    orderId: notification.OrderId,
                    discountApply: notification.DiscountApply
                );

                var voucher = await unitOfWork.Repository<Voucher>().FindByIdAsync(notification.VoucherId, cancellationToken);
                --voucher.TotalQuantity;
                ++voucher.UsedQuantity;

                await unitOfWork.Repository<VoucherUsage>().AddAsync(usage, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling VoucherUsageEvent");
                await unitOfWork.RollbackAsync(cancellationToken); // thêm check null/connection bên trong
                throw;
            }
        }

    }
}
