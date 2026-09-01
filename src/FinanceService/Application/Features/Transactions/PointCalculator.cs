namespace Application.Features.Transactions;

public static class PointCalculator
{
    private const decimal CurrencyPerPoint = 1000m;

    public static decimal CalculateEarnedPoints(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

        return decimal.Ceiling(amount / CurrencyPerPoint);
    }
}
