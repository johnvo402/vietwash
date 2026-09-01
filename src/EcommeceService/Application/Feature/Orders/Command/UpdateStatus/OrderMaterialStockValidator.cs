namespace Application.Feature.Orders.Command.UpdateStatus;

public static class OrderMaterialStockValidator
{
    public static MaterialStockValidationResult Validate(
        IReadOnlyCollection<MaterialRequirement> requirements,
        IReadOnlyCollection<MaterialStockSnapshot> stocks
    )
    {
        try
        {
            Dictionary<long, decimal> availableByProduct = stocks
                .GroupBy(x => x.BranchProductId)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.AvailableBaseQuantity));

            foreach (
                var product in requirements
                    .GroupBy(x => new { x.BranchProductId, x.BranchProductName })
                    .OrderBy(x => x.Key.BranchProductId)
            )
            {
                decimal required = product.Sum(x => x.RequiredBaseQuantity);
                decimal available = availableByProduct.GetValueOrDefault(
                    product.Key.BranchProductId
                );
                if (available < required)
                    return MaterialStockValidationResult.Failure(
                        product.Key.BranchProductId,
                        product.Key.BranchProductName,
                        required,
                        available
                    );
            }
        }
        catch (OverflowException)
        {
            return MaterialStockValidationResult.Failure(
                0,
                "unknown",
                decimal.MaxValue,
                0
            );
        }

        return MaterialStockValidationResult.Success();
    }
}

public sealed record MaterialStockSnapshot(long BranchProductId, decimal AvailableBaseQuantity);

public sealed record MaterialStockValidationResult
{
    public bool IsSuccess { get; private init; }
    public long BranchProductId { get; private init; }
    public string? BranchProductName { get; private init; }
    public decimal RequiredBaseQuantity { get; private init; }
    public decimal AvailableBaseQuantity { get; private init; }

    public string ErrorMessage =>
        $"Insufficient stock for {BranchProductName}. Required: {RequiredBaseQuantity}; Available: {AvailableBaseQuantity}.";

    public static MaterialStockValidationResult Success() => new() { IsSuccess = true };

    public static MaterialStockValidationResult Failure(
        long branchProductId,
        string branchProductName,
        decimal requiredBaseQuantity,
        decimal availableBaseQuantity
    ) =>
        new()
        {
            BranchProductId = branchProductId,
            BranchProductName = branchProductName,
            RequiredBaseQuantity = requiredBaseQuantity,
            AvailableBaseQuantity = availableBaseQuantity,
        };
}
