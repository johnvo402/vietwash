using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Events;
using Mediator;
using Serilog;

namespace Application.Common.HandleEventDomains
{
    public class CreateVoucherUsageEventHandler(IUnitOfWork unitOfWork, ILogger logger)
        : INotificationHandler<VoucherUsageEvent>
    {
        public async ValueTask Handle(
            VoucherUsageEvent notification,
            CancellationToken cancellationToken
        )
        {
            try
            {
                logger.Information(
                    "Handling VoucherUsageEvent for OrderId: {@OrderId}",
                    notification.OrderId
                );

                if (
                    await unitOfWork
                        .Repository<VoucherUsage>()
                        .AnyAsync(x => x.OrderId == notification.OrderId, cancellationToken)
                )
                    return;

                var usage = new VoucherUsage(
                    voucherId: notification.VoucherId,
                    customerId: notification.CustomerId,
                    orderId: notification.OrderId,
                    discountApply: notification.DiscountApply
                );

                await unitOfWork.Repository<VoucherUsage>().AddAsync(usage, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling VoucherUsageEvent");
                throw;
            }
        }
    }
}
