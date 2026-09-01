using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Utils;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.UpdateStatus;

public static class OrderMaterialConsumption
{
    public static async Task<OrderMaterialConsumptionResult> ConsumeAsync(
        IUnitOfWork unitOfWork,
        Order order,
        CancellationToken cancellationToken
    )
    {
        bool alreadyConsumed = await unitOfWork
            .Repository<InventoryDocument>()
            .QueryAsync(x => x.SourceOrderId == order.Id)
            .AnyAsync(cancellationToken);
        if (alreadyConsumed)
            return OrderMaterialConsumptionResult.Success();

        long[] branchProductIds = await unitOfWork
            .Repository<OrderItem>()
            .QueryAsync(x => x.OrderId == order.Id)
            .SelectMany(x =>
                x.UnitRelation.AsUnitRelation.Select(resource => resource.ProductId)
            )
            .Distinct()
            .OrderBy(x => x)
            .ToArrayAsync(cancellationToken);

        if (branchProductIds.Length == 0)
            return OrderMaterialConsumptionResult.Success();

        BranchProductLockPlan lockPlan = BranchProductLockPlan.Create(branchProductIds);
        _ = await unitOfWork.ExecuteSqlCommandAsync(
            lockPlan.Sql,
            lockPlan.Parameters,
            cancellationToken
        );

        List<OrderMaterialInput> inputs = await LoadInputs(
            unitOfWork,
            order.Id,
            cancellationToken
        );
        long[] currentProductIds = inputs
            .Select(x => x.BranchProductId)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        if (!currentProductIds.SequenceEqual(lockPlan.BranchProductIds))
            return OrderMaterialConsumptionResult.Failure(
                "The order material definition changed concurrently. Please retry."
            );

        OrderMaterialResolution resolution = OrderMaterialRequirementResolver.Resolve(
            order.BranchId,
            inputs
        );
        if (!resolution.IsSuccess)
            return OrderMaterialConsumptionResult.Failure(
                resolution.ErrorMessage ?? "Material requirement is invalid."
            );

        if (resolution.Requirements.Count == 0)
            return OrderMaterialConsumptionResult.Success();

        long[] resolvedProductIds = resolution
            .Requirements.Select(x => x.BranchProductId)
            .Distinct()
            .ToArray();
        List<MaterialStockSnapshot> stocks = await unitOfWork
            .Repository<ProductSupplying>()
            .QueryAsync(x =>
                resolvedProductIds.Contains(x.ProductId)
                && x.InventoryDocument.Status == InventoryStatus.Completed
            )
            .GroupBy(x => x.ProductId)
            .Select(x => new MaterialStockSnapshot(
                x.Key,
                x.Sum(supplying => supplying.Quantity * supplying.UnitRelation.Multiple)
            ))
            .ToListAsync(cancellationToken);

        MaterialStockValidationResult stockValidation =
            OrderMaterialStockValidator.Validate(resolution.Requirements, stocks);
        if (!stockValidation.IsSuccess)
            return OrderMaterialConsumptionResult.Failure(stockValidation.ErrorMessage);

        InventoryDocument export;
        try
        {
            export = OrderMaterialExportFactory.Create(
                order,
                resolution.Requirements,
                Generator.GenerateCode("XH", 6),
                DateTimeOffset.UtcNow
            )!;
        }
        catch (OverflowException)
        {
            return OrderMaterialConsumptionResult.Failure(
                "Material export amount is outside the supported numeric range."
            );
        }

        await unitOfWork
            .Repository<InventoryDocument>()
            .AddAsync(export, cancellationToken);
        return OrderMaterialConsumptionResult.Success(export);
    }

    private static Task<List<OrderMaterialInput>> LoadInputs(
        IUnitOfWork unitOfWork,
        long orderId,
        CancellationToken cancellationToken
    ) =>
        unitOfWork
            .Repository<OrderItem>()
            .QueryAsync(x => x.OrderId == orderId)
            .SelectMany(orderItem =>
                orderItem.UnitRelation.AsUnitRelation.Select(resource =>
                    new OrderMaterialInput(
                        orderItem.ServiceId,
                        orderItem.UnitRelation.ServiceId,
                        orderItem.UnitRelation.Status,
                        orderItem.UnitRelation.BaseUnit,
                        orderItem.UnitRelation.Multiple,
                        orderItem.Quantity,
                        resource.ProductId,
                        resource.BranchProduct.Name,
                        resource.BranchProduct.BranchId,
                        resource.BranchProduct.Status,
                        resource.BranchProduct.Disable,
                        resource.BranchProduct.CapitalPrice,
                        resource.UnitProductId,
                        resource.UnitProduct.BranchProductId,
                        resource.UnitProduct.Status,
                        resource.UnitProduct.BaseUnit,
                        resource.UnitProduct.Multiple,
                        resource.Quantity
                    )
                )
            )
            .ToListAsync(cancellationToken);
}

public static class OrderMaterialExportFactory
{
    public static InventoryDocument? Create(
        Order order,
        IReadOnlyCollection<MaterialRequirement> requirements,
        string code,
        DateTimeOffset transactionAt
    )
    {
        if (requirements.Count == 0)
            return null;

        decimal amount = 0;
        foreach (MaterialRequirement requirement in requirements)
            amount = checked(amount + requirement.Cost);

        InventoryDocument export = InventoryDocument.CreateOrderMaterialExport(
            code,
            amount,
            order.BranchId,
            order.Id,
            transactionAt,
            $"Phiếu xuất cho đơn hàng #{order.Code}"
        );
        foreach (MaterialRequirement requirement in requirements)
        {
            export.ProductSupplyings.Add(
                new ProductSupplying
                {
                    ProductId = requirement.BranchProductId,
                    UnitRelationId = requirement.UnitProductId,
                    Price = requirement.CapitalPrice,
                    SupplierId = null,
                    Quantity = -requirement.RequiredQuantity,
                }
            );
        }

        return export;
    }
}

public sealed record OrderMaterialConsumptionResult
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public InventoryDocument? ExportDocument { get; private init; }

    public static OrderMaterialConsumptionResult Success(
        InventoryDocument? exportDocument = null
    ) => new() { IsSuccess = true, ExportDocument = exportDocument };

    public static OrderMaterialConsumptionResult Failure(string errorMessage) =>
        new() { ErrorMessage = errorMessage };
}
