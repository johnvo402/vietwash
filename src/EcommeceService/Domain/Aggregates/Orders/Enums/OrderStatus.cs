namespace Domain.Aggregates.Orders.Enums
{
    public enum OrderStatus : byte
    {
        Pending = 1,
        InProgress = 2,
        Processed = 3,
        Completed = 4,
        Cancelled = 5,
    }
}
