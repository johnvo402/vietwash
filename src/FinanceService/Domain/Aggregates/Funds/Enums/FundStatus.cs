namespace Domain.Aggregates.Funds.Enums
{
    public enum FundStatus : byte
    {
        PendingConfirmation = 1,
        Confirmed = 2,
        Cancelled = 3,
    }
}
