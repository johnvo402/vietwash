using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Orders.Events;
using Mediator;

namespace Application.Common.HandleEventDomains.Orders
{
    public class UseEquipmentOrderHandler : INotificationHandler<UseEquipmentOrder>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UseEquipmentOrderHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask Handle(
            UseEquipmentOrder notification,
            CancellationToken cancellationToken
        )
        {
            try
            {
                foreach (var x in notification.OrderEquipments)
                {
                    var equipment = await _unitOfWork
                        .Repository<Equipment>()
                        .FindByIdAsync(x.EquipmentId, cancellationToken);

                    if (equipment != null)
                    {
                        equipment.Using = notification.Using;
                        await _unitOfWork.Repository<Equipment>().UpdateAsync(equipment);
                    }
                }

                await _unitOfWork.SaveAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
