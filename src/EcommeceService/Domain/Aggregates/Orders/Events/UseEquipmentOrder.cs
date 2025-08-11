using Mediator;

namespace Domain.Aggregates.Orders.Events
{
    public class UseEquipmentOrder : INotification
    {
        public List<OrderEquipment> OrderEquipments { get; set; }
    }
}
