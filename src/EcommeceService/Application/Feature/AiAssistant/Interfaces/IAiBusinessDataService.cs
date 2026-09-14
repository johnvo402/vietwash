using Application.Feature.AiAssistant.Models;

namespace Application.Feature.AiAssistant.Interfaces;

public interface IAiBusinessDataService
{
    Task<RevenueSummaryResult> GetRevenueSummaryAsync(
        GetRevenueSummaryInput input,
        CancellationToken cancellationToken
    );

    Task<OrderStatisticsResult> GetOrderStatisticsAsync(
        GetOrderStatisticsInput input,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    );

    Task<TopServicesResult> GetTopServicesAsync(
        GetTopServicesInput input,
        CancellationToken cancellationToken
    );

    Task<InventoryAlertsResult> GetInventoryAlertsAsync(
        GetInventoryAlertsInput input,
        CancellationToken cancellationToken
    );

    Task<EquipmentUtilizationResult> GetEquipmentUtilizationAsync(
        GetEquipmentUtilizationInput input,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    );
}
