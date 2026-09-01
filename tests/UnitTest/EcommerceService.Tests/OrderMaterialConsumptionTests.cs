using Application.Common.HandleEventDomains.Inventories;
using Application.Feature.Orders.Command.UpdateStatus;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace EcommerceService.Tests;

public class OrderMaterialConsumptionTests
{
    private static readonly DateTimeOffset TransactionAt = new(
        2026,
        9,
        1,
        12,
        0,
        0,
        TimeSpan.Zero
    );

    [Fact]
    public void ServiceWithoutResources_CreatesNeitherRequirementNorExport()
    {
        OrderMaterialResolution resolution = Resolve([]);

        Assert.True(resolution.IsSuccess);
        Assert.Empty(resolution.Requirements);
        Assert.Null(
            OrderMaterialExportFactory.Create(
                CreateOrder(),
                resolution.Requirements,
                "XH000001",
                TransactionAt
            )
        );
    }

    [Fact]
    public void OneResource_CreatesTheExpectedRequirement()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve([ValidInput(resourceQuantity: 20m)]).Requirements
        );

        Assert.Equal(20m, requirement.RequiredQuantity);
        Assert.Equal(20m, requirement.RequiredBaseQuantity);
    }

    [Fact]
    public void OrderQuantity_MultipliesResourceConsumption()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve([ValidInput(resourceQuantity: 20m, orderQuantity: 5)]).Requirements
        );

        Assert.Equal(100m, requirement.RequiredQuantity);
    }

    [Fact]
    public void BaseServiceUnit_DoesNotApplyItsMultiple()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve(
                [
                    ValidInput(resourceQuantity: 20m) with
                    {
                        ServiceUnitBaseUnit = true,
                        ServiceUnitMultiple = 99,
                    },
                ]
            ).Requirements
        );

        Assert.Equal(20m, requirement.RequiredQuantity);
    }

    [Fact]
    public void NonBaseServiceUnit_AppliesItsMultipleExactlyOnce()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve(
                [
                    ValidInput(resourceQuantity: 20m) with
                    {
                        ServiceUnitBaseUnit = false,
                        ServiceUnitMultiple = 5,
                    },
                ]
            ).Requirements
        );

        Assert.Equal(100m, requirement.RequiredQuantity);
    }

    [Fact]
    public void MaterialUnit_ConvertsSelectedQuantityToBaseQuantity()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve(
                [
                    ValidInput(resourceQuantity: 0.5m) with
                    {
                        MaterialUnitBaseUnit = false,
                        MaterialUnitMultiple = 1000,
                    },
                ]
            ).Requirements
        );

        Assert.Equal(0.5m, requirement.RequiredQuantity);
        Assert.Equal(500m, requirement.RequiredBaseQuantity);
    }

    [Fact]
    public void SameProductAndMaterialUnit_ConsolidatesIntoOneLine()
    {
        OrderMaterialResolution resolution = Resolve(
            [ValidInput(resourceQuantity: 2m), ValidInput(resourceQuantity: 3m)]
        );

        MaterialRequirement requirement = Assert.Single(resolution.Requirements);
        Assert.Equal(5m, requirement.RequiredQuantity);
    }

    [Fact]
    public void SameProductWithDifferentMaterialUnits_RemainsSeparate()
    {
        OrderMaterialResolution resolution = Resolve(
            [
                ValidInput(resourceQuantity: 2m),
                ValidInput(resourceQuantity: 3m) with { MaterialUnitId = 301 },
            ]
        );

        Assert.Equal(2, resolution.Requirements.Count);
    }

    [Fact]
    public void CrossBranchMaterial_IsRejected()
    {
        OrderMaterialResolution resolution = Resolve(
            [ValidInput() with { BranchProductBranchId = 99 }]
        );

        Assert.False(resolution.IsSuccess);
        Assert.Equal(OrderMaterialFailure.CrossBranchProduct, resolution.FailureReason);
    }

    [Fact]
    public void InactiveMaterial_IsRejected()
    {
        OrderMaterialResolution resolution = Resolve(
            [ValidInput() with { BranchProductStatus = ActivationStatus.Inactive }]
        );

        Assert.False(resolution.IsSuccess);
        Assert.Equal(OrderMaterialFailure.InactiveProduct, resolution.FailureReason);
    }

    [Fact]
    public void DisabledMaterial_IsRejected()
    {
        OrderMaterialResolution resolution = Resolve(
            [ValidInput() with { BranchProductDisabled = true }]
        );

        Assert.False(resolution.IsSuccess);
        Assert.Equal(OrderMaterialFailure.DisabledProduct, resolution.FailureReason);
    }

    [Fact]
    public void InactiveMaterialUnit_IsRejected()
    {
        OrderMaterialResolution resolution = Resolve(
            [ValidInput() with { MaterialUnitStatus = ActivationStatus.Inactive }]
        );

        Assert.False(resolution.IsSuccess);
        Assert.Equal(OrderMaterialFailure.InactiveMaterialUnit, resolution.FailureReason);
    }

    [Fact]
    public void MaterialUnitFromAnotherProduct_IsRejected()
    {
        OrderMaterialResolution resolution = Resolve(
            [ValidInput() with { MaterialUnitBranchProductId = 999 }]
        );

        Assert.False(resolution.IsSuccess);
        Assert.Equal(OrderMaterialFailure.InvalidMaterialUnit, resolution.FailureReason);
    }

    [Fact]
    public void EnoughBaseUnitStock_AllowsConsumption()
    {
        IReadOnlyList<MaterialRequirement> requirements = Resolve(
            [ValidInput(resourceQuantity: 10m)]
        ).Requirements;

        MaterialStockValidationResult result = OrderMaterialStockValidator.Validate(
            requirements,
            [new MaterialStockSnapshot(200, 10m)]
        );

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void StockTenAndRequirementEleven_IsRejected()
    {
        IReadOnlyList<MaterialRequirement> requirements = Resolve(
            [ValidInput(resourceQuantity: 11m)]
        ).Requirements;

        MaterialStockValidationResult result = OrderMaterialStockValidator.Validate(
            requirements,
            [new MaterialStockSnapshot(200, 10m)]
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(11m, result.RequiredBaseQuantity);
        Assert.Equal(10m, result.AvailableBaseQuantity);
    }

    [Fact]
    public void MissingCompletedLedgerStock_IsTreatedAsZero()
    {
        IReadOnlyList<MaterialRequirement> requirements = Resolve(
            [ValidInput(resourceQuantity: 1m)]
        ).Requirements;

        MaterialStockValidationResult result = OrderMaterialStockValidator.Validate(
            requirements,
            []
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(0m, result.AvailableBaseQuantity);
    }

    [Fact]
    public void RequirementsInDifferentUnits_AreComparedUsingBaseQuantities()
    {
        IReadOnlyList<MaterialRequirement> requirements = Resolve(
            [
                ValidInput(resourceQuantity: 500m),
                ValidInput(resourceQuantity: 0.5m) with
                {
                    MaterialUnitId = 301,
                    MaterialUnitBaseUnit = false,
                    MaterialUnitMultiple = 1000,
                },
            ]
        ).Requirements;

        MaterialStockValidationResult result = OrderMaterialStockValidator.Validate(
            requirements,
            [new MaterialStockSnapshot(200, 1000m)]
        );

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Export_UsesMaterialIdentifiersAndNegativeSelectedUnitQuantity()
    {
        Order order = CreateOrder();
        MaterialRequirement requirement = Assert.Single(
            Resolve([ValidInput(resourceQuantity: 2m)]).Requirements
        );

        InventoryDocument export = OrderMaterialExportFactory.Create(
            order,
            [requirement],
            "XH000001",
            TransactionAt
        )!;
        ProductSupplying line = Assert.Single(export.ProductSupplyings);

        Assert.Equal(InventoryType.Export, export.Type);
        Assert.Equal(InventoryStatus.Completed, export.Status);
        Assert.Equal(order.Id, export.SourceOrderId);
        Assert.Equal(200, line.ProductId);
        Assert.Equal(300, line.UnitRelationId);
        Assert.Equal(-2m, line.Quantity);
        Assert.Null(line.SupplierId);
    }

    [Fact]
    public void ExportAmount_UsesBaseQuantityTimesCapitalPrice()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve(
                [
                    ValidInput(resourceQuantity: 2m) with
                    {
                        MaterialUnitBaseUnit = false,
                        MaterialUnitMultiple = 1000,
                        CapitalPrice = 3m,
                    },
                ]
            ).Requirements
        );

        InventoryDocument export = OrderMaterialExportFactory.Create(
            CreateOrder(),
            [requirement],
            "XH000001",
            TransactionAt
        )!;

        Assert.Equal(6000m, export.Amount);
        Assert.Equal(3m, export.ProductSupplyings.Single().Price);
    }

    [Fact]
    public void CompletedExport_DecreasesLedgerByExactlyRequiredBaseQuantity()
    {
        MaterialRequirement requirement = Assert.Single(
            Resolve(
                [
                    ValidInput(resourceQuantity: 2m) with
                    {
                        MaterialUnitBaseUnit = false,
                        MaterialUnitMultiple = 4,
                    },
                ]
            ).Requirements
        );
        InventoryDocument export = OrderMaterialExportFactory.Create(
            CreateOrder(),
            [requirement],
            "XH000001",
            TransactionAt
        )!;
        ProductSupplying line = export.ProductSupplyings.Single();

        decimal finalStock = 10m + line.Quantity * 4m;

        Assert.Equal(2m, finalStock);
        Assert.Equal(8m, requirement.RequiredBaseQuantity);
    }

    [Fact]
    public void AutomaticOrderExport_SkipsSupplierAndNotificationSideEffects()
    {
        InventoryDocument export = OrderMaterialExportFactory.Create(
            CreateOrder(),
            Resolve([ValidInput()]).Requirements,
            "XH000001",
            TransactionAt
        )!;

        Assert.False(
            InventoryDocumentCompletionPolicy.ShouldRunExternalSideEffects(export)
        );
        Assert.All(export.ProductSupplyings, x => Assert.Null(x.SupplierId));
    }

    [Fact]
    public void ManualInventoryDocument_KeepsExistingCompletionSideEffects()
    {
        var document = new InventoryDocument(
            "NK000001",
            10,
            InventoryType.Import,
            10
        );

        Assert.True(
            InventoryDocumentCompletionPolicy.ShouldRunExternalSideEffects(document)
        );
    }

    [Fact]
    public void BranchProductLocks_AreDeduplicatedAndAcquiredInIdOrder()
    {
        BranchProductLockPlan plan = BranchProductLockPlan.Create([9, 2, 9, 5]);

        Assert.Equal([2, 5, 9], plan.BranchProductIds);
        Assert.Equal([2L, 5L, 9L], plan.Parameters.Cast<long>());
        Assert.Equal(
            "SELECT id FROM branch_product WHERE id IN ({0}, {1}, {2}) ORDER BY id FOR UPDATE",
            plan.Sql
        );
    }

    [Fact]
    public void SerializedConcurrentOrders_CannotOversellStock()
    {
        MaterialRequirement sevenUnits = Assert.Single(
            Resolve([ValidInput(resourceQuantity: 7m)]).Requirements
        );
        BranchProductLockPlan firstLock = BranchProductLockPlan.Create([200]);
        BranchProductLockPlan secondLock = BranchProductLockPlan.Create([200]);

        MaterialStockValidationResult first = OrderMaterialStockValidator.Validate(
            [sevenUnits],
            [new MaterialStockSnapshot(200, 10m)]
        );
        decimal stockAfterFirst = 10m - sevenUnits.RequiredBaseQuantity;
        MaterialStockValidationResult second = OrderMaterialStockValidator.Validate(
            [sevenUnits],
            [new MaterialStockSnapshot(200, stockAfterFirst)]
        );

        Assert.Equal(firstLock.Sql, secondLock.Sql);
        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(3m, stockAfterFirst);
        Assert.True(stockAfterFirst >= 0);
    }

    [Fact]
    public void FailedStockDecision_LeavesPendingOrderAndEquipmentFree()
    {
        Order order = CreateOrder();
        var equipment = new EquipmentSnapshot(
            1,
            "Washer",
            order.BranchId,
            Domain.Aggregates.Equipments.Enums.EquipmentStatus.Active,
            false
        );
        MaterialStockValidationResult stock = OrderMaterialStockValidator.Validate(
            Resolve([ValidInput(resourceQuantity: 11m)]).Requirements,
            [new MaterialStockSnapshot(200, 10m)]
        );

        Assert.False(stock.IsSuccess);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.False(equipment.Using);
    }

    [Fact]
    public void StartingWithoutResources_StillAllowsOrderTransition()
    {
        Order order = CreateOrder();
        Assert.Empty(Resolve([]).Requirements);

        OrderTransitionResult result = order.TransitionTo(
            OrderStatus.InProgress,
            orderEquipments: [new OrderEquipment { EquipmentId = 1, EquipmentName = "Washer" }]
        );

        Assert.Equal(OrderTransitionResult.Applied, result);
        Assert.Equal(OrderStatus.InProgress, order.Status);
    }

    [Fact]
    public void RepeatedInProgressTransition_DoesNotAuthorizeAnotherExport()
    {
        Order order = CreateOrder(OrderStatus.InProgress);

        OrderTransitionResult result = order.EvaluateTransition(
            OrderStatus.InProgress,
            paymentMethod: null,
            equipmentCount: 0
        );

        Assert.Equal(OrderTransitionResult.Idempotent, result);
        Assert.Empty(order.UncommittedEvents);
    }

    [Fact]
    public void CancellingAfterStart_DoesNotCreateACompensatingInventoryEvent()
    {
        Order order = CreateOrder(OrderStatus.InProgress);

        OrderTransitionResult result = order.TransitionTo(OrderStatus.Cancelled);

        Assert.Equal(OrderTransitionResult.Applied, result);
        Assert.DoesNotContain(
            order.UncommittedEvents,
            domainEvent => domainEvent.GetType().Name.Contains("Inventory")
        );
    }

    private static OrderMaterialResolution Resolve(
        IReadOnlyCollection<OrderMaterialInput> inputs
    ) => OrderMaterialRequirementResolver.Resolve(10, inputs);

    private static OrderMaterialInput ValidInput(
        decimal resourceQuantity = 1m,
        int orderQuantity = 1
    ) =>
        new(
            OrderItemServiceId: 100,
            ServiceUnitServiceId: 100,
            ServiceUnitStatus: ActivationStatus.Active,
            ServiceUnitBaseUnit: true,
            ServiceUnitMultiple: 1,
            OrderQuantity: orderQuantity,
            BranchProductId: 200,
            BranchProductName: "Detergent",
            BranchProductBranchId: 10,
            BranchProductStatus: ActivationStatus.Active,
            BranchProductDisabled: false,
            CapitalPrice: 3m,
            MaterialUnitId: 300,
            MaterialUnitBranchProductId: 200,
            MaterialUnitStatus: ActivationStatus.Active,
            MaterialUnitBaseUnit: true,
            MaterialUnitMultiple: 1,
            ResourceQuantity: resourceQuantity
        );

    private static Order CreateOrder(OrderStatus status = OrderStatus.Pending) =>
        new(
            branchId: 10,
            staffId: 20,
            code: "OD000100",
            amount: 100,
            total: 100,
            status: status
        )
        {
            Id = 100,
        };
}
