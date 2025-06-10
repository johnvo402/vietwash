using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Events;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Serilog;
using System.Data.Common;


namespace Application.Common.HandleEventDomains
{
    public class UpdateStatusOrderEventHandler(ILogger logger, IUnitOfWork unitOfWork) : INotificationHandler<UpdateStatusOrderEvent>
    {
        public async ValueTask Handle(UpdateStatusOrderEvent notification, CancellationToken cancellationToken)
        {
            var fund = new Fund
            {
                TypeId = notification.TypeId,
                BehaviorId = notification.BehaviorId,
                Amount = notification.Amount,
                PaymentMethod = notification.PaymentMethod,
                ReferenceId = notification.ReferenceId,
                TransactionDate = DateTimeOffset.UtcNow,
                Name = $"Fund for Order - {notification.TypeId}", // Gán giá trị cho Name
                Note = $"Created due to order status update with BehaviorId: {notification.BehaviorId}", // Gán giá trị cho Note
                Code = $"FU-{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..]}"
            };
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Fund>().AddAsync(fund, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
