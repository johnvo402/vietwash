namespace Application.Feature.AiAssistant.Models;

public sealed record GetRevenueSummaryInput(long BranchId, DateOnly From, DateOnly To);

public sealed record DailyRevenue(DateOnly Date, decimal Revenue);

public sealed record RevenueSummaryResult(
    long BranchId,
    DateOnly From,
    DateOnly To,
    decimal TotalRevenue,
    IReadOnlyList<DailyRevenue> DailyRevenue
);

public sealed record GetOrderStatisticsInput(long BranchId, DateOnly From, DateOnly To);

public sealed record OrderStatusCount(string Status, int Count);

public sealed record OrderStatisticsResult(
    long BranchId,
    DateOnly From,
    DateOnly To,
    int TotalOrders,
    IReadOnlyList<OrderStatusCount> ByStatus
);

public sealed record GetTopServicesInput(long BranchId, DateOnly From, DateOnly To, int Limit = 10);

public sealed record TopServiceItem(string Name, int UsageCount, decimal GrossRevenue);

public sealed record TopServicesResult(
    long BranchId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<TopServiceItem> Services
);

public sealed record GetInventoryAlertsInput(long BranchId, decimal LowStockThreshold = 10m);

public sealed record InventoryAlertItem(string Name, string? Sku, decimal CurrentStock);

public sealed record InventoryAlertsResult(
    long BranchId,
    decimal LowStockThreshold,
    IReadOnlyList<InventoryAlertItem> Items
);

public sealed record GetEquipmentUtilizationInput(long BranchId, DateOnly From, DateOnly To);

public sealed record EquipmentUtilizationResult(
    long BranchId,
    DateOnly From,
    DateOnly To,
    int TotalEquipment,
    int ActiveEquipment,
    int CurrentlyInUse,
    int DistinctEquipmentUsed,
    int AssignmentCount,
    decimal UtilizationPercent
);
