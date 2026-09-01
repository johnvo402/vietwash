using Domain.Aggregates.Enums;

namespace Application.Feature.Orders.Command.UpdateStatus;

public static class OrderMaterialRequirementResolver
{
    public static OrderMaterialResolution Resolve(
        long orderBranchId,
        IReadOnlyCollection<OrderMaterialInput> inputs
    )
    {
        var requirements = new Dictionary<(long ProductId, long UnitId), MaterialRequirement>();

        try
        {
            foreach (OrderMaterialInput input in inputs)
            {
                OrderMaterialResolution? invalid = Validate(orderBranchId, input);
                if (invalid is not null)
                    return invalid;

                decimal serviceFactor = input.ServiceUnitBaseUnit
                    ? 1m
                    : input.ServiceUnitMultiple;
                decimal requiredQuantity = checked(
                    input.ResourceQuantity * serviceFactor * input.OrderQuantity
                );
                decimal requiredBaseQuantity = checked(
                    requiredQuantity * input.MaterialUnitMultiple
                );
                decimal cost = checked(requiredBaseQuantity * input.CapitalPrice);
                var key = (input.BranchProductId, input.MaterialUnitId);

                if (requirements.TryGetValue(key, out MaterialRequirement? current))
                {
                    requirements[key] = current with
                    {
                        RequiredQuantity = checked(
                            current.RequiredQuantity + requiredQuantity
                        ),
                        RequiredBaseQuantity = checked(
                            current.RequiredBaseQuantity + requiredBaseQuantity
                        ),
                        Cost = checked(current.Cost + cost),
                    };
                }
                else
                {
                    requirements.Add(
                        key,
                        new MaterialRequirement(
                            input.BranchProductId,
                            input.BranchProductName,
                            input.MaterialUnitId,
                            requiredQuantity,
                            requiredBaseQuantity,
                            input.CapitalPrice,
                            cost
                        )
                    );
                }
            }
        }
        catch (OverflowException)
        {
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InvalidQuantity,
                "Material requirement is outside the supported numeric range."
            );
        }

        return OrderMaterialResolution.Success(
            requirements
                .Values.OrderBy(x => x.BranchProductId)
                .ThenBy(x => x.UnitProductId)
                .ToArray()
        );
    }

    private static OrderMaterialResolution? Validate(
        long orderBranchId,
        OrderMaterialInput input
    )
    {
        if (
            input.OrderQuantity <= 0
            || input.ResourceQuantity <= 0
            || input.BranchProductId <= 0
            || input.MaterialUnitId <= 0
        )
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InvalidQuantity,
                "Material requirement contains an invalid quantity or identifier."
            );

        if (
            input.ServiceUnitServiceId != input.OrderItemServiceId
            || input.ServiceUnitStatus != ActivationStatus.Active
            || (!input.ServiceUnitBaseUnit && input.ServiceUnitMultiple <= 0)
        )
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InvalidServiceUnit,
                "The order service unit is inactive or invalid."
            );

        if (input.BranchProductBranchId != orderBranchId)
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.CrossBranchProduct,
                $"Material {input.BranchProductName} belongs to another branch."
            );

        if (input.BranchProductStatus != ActivationStatus.Active)
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InactiveProduct,
                $"Material {input.BranchProductName} is inactive."
            );

        if (input.BranchProductDisabled)
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.DisabledProduct,
                $"Material {input.BranchProductName} is disabled."
            );

        if (
            input.MaterialUnitBranchProductId != input.BranchProductId
            || input.MaterialUnitMultiple <= 0
            || (input.MaterialUnitBaseUnit && input.MaterialUnitMultiple != 1)
        )
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InvalidMaterialUnit,
                $"The selected unit for material {input.BranchProductName} is invalid."
            );

        if (input.MaterialUnitStatus != ActivationStatus.Active)
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InactiveMaterialUnit,
                $"The selected unit for material {input.BranchProductName} is inactive."
            );

        if (input.CapitalPrice < 0)
            return OrderMaterialResolution.Failure(
                OrderMaterialFailure.InvalidCapitalPrice,
                $"Material {input.BranchProductName} has an invalid capital price."
            );

        return null;
    }
}

public sealed record OrderMaterialInput(
    long OrderItemServiceId,
    long? ServiceUnitServiceId,
    ActivationStatus ServiceUnitStatus,
    bool ServiceUnitBaseUnit,
    decimal ServiceUnitMultiple,
    int OrderQuantity,
    long BranchProductId,
    string BranchProductName,
    long BranchProductBranchId,
    ActivationStatus BranchProductStatus,
    bool BranchProductDisabled,
    decimal CapitalPrice,
    long MaterialUnitId,
    long? MaterialUnitBranchProductId,
    ActivationStatus MaterialUnitStatus,
    bool MaterialUnitBaseUnit,
    decimal MaterialUnitMultiple,
    decimal ResourceQuantity
);

public sealed record MaterialRequirement(
    long BranchProductId,
    string BranchProductName,
    long UnitProductId,
    decimal RequiredQuantity,
    decimal RequiredBaseQuantity,
    decimal CapitalPrice,
    decimal Cost
);

public sealed record OrderMaterialResolution
{
    public bool IsSuccess { get; private init; }
    public IReadOnlyList<MaterialRequirement> Requirements { get; private init; } = [];
    public OrderMaterialFailure FailureReason { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static OrderMaterialResolution Success(
        IReadOnlyList<MaterialRequirement> requirements
    ) => new() { IsSuccess = true, Requirements = requirements };

    public static OrderMaterialResolution Failure(
        OrderMaterialFailure failureReason,
        string errorMessage
    ) =>
        new()
        {
            FailureReason = failureReason,
            ErrorMessage = errorMessage,
        };
}

public enum OrderMaterialFailure
{
    None,
    InvalidQuantity,
    InvalidServiceUnit,
    CrossBranchProduct,
    InactiveProduct,
    DisabledProduct,
    InvalidMaterialUnit,
    InactiveMaterialUnit,
    InvalidCapitalPrice,
}
