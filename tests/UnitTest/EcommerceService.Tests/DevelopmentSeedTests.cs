using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Infrastructure.Data;

namespace EcommerceService.Tests;

public class DevelopmentSeedTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InventoryFactory_ExpandsQuantityAndPreservesBranchAndMetadata()
    {
        var document = Import();
        var equipments = InventoryEquipmentFactory.Create(document, Now);
        Assert.Equal(new[] { "WM", "WM1", "WM2" }, equipments.Select(x => x.Code));
        Assert.All(equipments, equipment =>
        {
            Assert.Equal(2, equipment.BranchId);
            Assert.Equal(EquipmentStatus.Active, equipment.Status);
            Assert.False(equipment.Using);
            Assert.Equal("Washer", equipment.Name);
            Assert.Equal("washer.png", equipment.Image);
            Assert.Equal(document.Code, equipment.Description);
            Assert.Equal(Now.AddMonths(6), equipment.NextMaintenanceDate);
            Assert.Empty(equipment.UncommittedEvents);
        });
        Assert.Empty(document.UncommittedEvents);
    }

    [Fact]
    public void Reconcile_AddsOnlyMissingCaseInsensitiveBranchIdentities_ThenAddsNothing()
    {
        var existing = new List<Equipment> { Equipment(1, 2, "wm"), Equipment(2, 1, "WM1") };
        var missing = DevelopmentSeedPolicy.MissingEquipment([Import()], existing, Now);
        Assert.Equal(new[] { "WM1", "WM2" }, missing.Select(x => x.Code));
        existing.AddRange(missing);
        Assert.Empty(DevelopmentSeedPolicy.MissingEquipment([Import()], existing, Now));
    }

    [Fact]
    public void Reconcile_RecognizesLegacySeedReceipts_ButIgnoresOperationalInventory()
    {
        var legacy = Import();
        legacy.Code = "IM123456";
        legacy.Note = "Phiếu nhập hàng tháng 01/2025";
        Assert.Equal(3, DevelopmentSeedPolicy.MissingEquipment([legacy], [], Now).Count);
        legacy.Note = "Actual supplier delivery";
        Assert.Empty(DevelopmentSeedPolicy.MissingEquipment([legacy], [], Now));
        legacy.Code = "DEV-IM-B2-202501";
        legacy.Status = InventoryStatus.Pending;
        Assert.Empty(DevelopmentSeedPolicy.MissingEquipment([legacy], [], Now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    [InlineData(99L)]
    public void SeedMissingOrInvalidBranch_FailsWithoutFallingBack(long? branchId)
    {
        var document = Import();
        document.BranchId = branchId;
        var error = Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.MissingEquipment([document], [], Now));
        Assert.Contains("BranchId", error.Message);
    }

    [Fact]
    public void RuntimeFactory_CanRetainItsExplicitLegacyFallback()
    {
        var document = Import();
        document.BranchId = null;
        Assert.Throws<InvalidOperationException>(() => InventoryEquipmentFactory.Create(document, Now));
        Assert.All(InventoryEquipmentFactory.Create(document, Now, fallbackBranchId: 1), x => Assert.Equal(1, x.BranchId));
    }

    [Fact]
    public void OrderSelection_IsSameBranchActiveDistinctAndReserved()
    {
        Equipment[] equipments = [Equipment(1, 1), Equipment(2, 2), Equipment(3, 2), Equipment(4, 2)];
        equipments[3].Status = EquipmentStatus.UnderMaintenance;
        var reserved = new HashSet<long>();
        var first = Assert.Single(DevelopmentSeedPolicy.SelectEquipment(2, OrderStatus.InProgress, equipments, reserved, new Random(1)));
        var second = Assert.Single(DevelopmentSeedPolicy.SelectEquipment(2, OrderStatus.InProgress, equipments, reserved, new Random(1)));
        Assert.NotEqual(first.Id, second.Id);
        Assert.All(new[] { first, second }, x => { Assert.Equal(2, x.BranchId); Assert.True(x.Using); });
        Assert.Equal(2, reserved.Count);
        Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.SelectEquipment(2, OrderStatus.InProgress, equipments, reserved, new Random(1)));
    }

    [Fact]
    public void EmptyBranch_ReturnsDescriptiveInvariant_NotRandomFailure()
    {
        var error = Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.SelectEquipment(3, OrderStatus.InProgress, [], new HashSet<long>(), new Random(1)));
        Assert.Contains("No active seed equipment exists for Branch 3", error.Message);
        Assert.Empty(DevelopmentSeedPolicy.SelectEquipment(3, OrderStatus.Pending, [], new HashSet<long>(), new Random(1)));
    }

    [Theory]
    [InlineData(OrderStatus.Processed)]
    [InlineData(OrderStatus.Completed)]
    public void HistoricalOrders_DoNotClaimEquipment(OrderStatus status)
    {
        var equipment = Equipment(10, 2);
        DevelopmentSeedPolicy.SelectEquipment(2, status, [equipment], new HashSet<long>(), new Random(1));
        Assert.False(equipment.Using);
        // History must not release an unrelated active order's current claim either.
        equipment.Using = true;
        DevelopmentSeedPolicy.SelectEquipment(2, status, [equipment], new HashSet<long>(), new Random(1));
        Assert.True(equipment.Using);
    }

    [Fact]
    public void OrderValidation_RejectsMissingCrossBranchAndSharedActiveEquipment()
    {
        var order = Order();
        Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.ValidateOrders([order], []));
        order.OrderEquipments.Add(new OrderEquipment { EquipmentId = 10 });
        Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.ValidateOrders([order], [Equipment(10, 1)]));
        var equipment = Equipment(10, 2);
        Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.ValidateOrders([order], [equipment]));
        equipment.Using = true;
        var second = Order();
        second.OrderEquipments.Add(new OrderEquipment { EquipmentId = 10 });
        Assert.Throws<InvalidOperationException>(() => DevelopmentSeedPolicy.ValidateOrders([order, second], [equipment]));
        DevelopmentSeedPolicy.ValidateOrders([order], [equipment]);
    }

    private static Order Order() => new(2, 7, "DEV-OD", 100, 110, OrderStatus.InProgress);
    private static Equipment Equipment(long id, long branchId, string code = "EQ") => new(branchId, "Washer", code, 100, EquipmentStatus.Active) { Id = id };
    private static InventoryDocument Import() => new("DEV-IM-B2-202501", 300, InventoryType.Import, 2)
    {
        Status = InventoryStatus.Completed,
        TransactionAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
        EquipmentSupplyings = [new() { Code = "WM", Name = "Washer", Quantity = 3, Price = 100, Image = "washer.png" }],
    };
}
