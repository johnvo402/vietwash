using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Feature.AiAssistant.Interfaces;
using Application.Feature.AiAssistant.Models;

namespace Application.Feature.AiAssistant.Tools;

public sealed class AiBusinessToolRegistry(IAiBusinessDataService dataService)
    : IAiBusinessToolRegistry
{
    public const string RevenueSummary = "get_revenue_summary";
    public const string OrderStatistics = "get_order_statistics";
    public const string TopServices = "get_top_services";
    public const string InventoryAlerts = "get_inventory_alerts";
    public const string EquipmentUtilization = "get_equipment_utilization";

    private const int MaximumDateRangeDays = 366;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    public IReadOnlyList<AiToolDefinition> Definitions { get; } =
    [
        Definition(
            RevenueSummary,
            "Tổng hợp doanh thu đã thu theo ngày của một chi nhánh trong khoảng ngày địa phương.",
            """
            {"type":"object","properties":{"branchId":{"type":"integer"},"from":{"type":"string","description":"Ngày bắt đầu dạng YYYY-MM-DD"},"to":{"type":"string","description":"Ngày kết thúc dạng YYYY-MM-DD"}},"required":["branchId","from","to"]}
            """
        ),
        Definition(
            OrderStatistics,
            "Thống kê số đơn được tạo, phân theo trạng thái, cho một chi nhánh và khoảng ngày địa phương.",
            """
            {"type":"object","properties":{"branchId":{"type":"integer"},"from":{"type":"string","description":"Ngày bắt đầu dạng YYYY-MM-DD"},"to":{"type":"string","description":"Ngày kết thúc dạng YYYY-MM-DD"}},"required":["branchId","from","to"]}
            """
        ),
        Definition(
            TopServices,
            "Các dịch vụ có doanh thu gộp cao nhất từ đơn hoàn tất của một chi nhánh.",
            """
            {"type":"object","properties":{"branchId":{"type":"integer"},"from":{"type":"string","description":"Ngày bắt đầu dạng YYYY-MM-DD"},"to":{"type":"string","description":"Ngày kết thúc dạng YYYY-MM-DD"},"limit":{"type":"integer","minimum":1,"maximum":10}},"required":["branchId","from","to"]}
            """
        ),
        Definition(
            InventoryAlerts,
            "Danh sách nguyên liệu có tồn kho nhỏ hơn hoặc bằng ngưỡng tại một chi nhánh.",
            """
            {"type":"object","properties":{"branchId":{"type":"integer"},"lowStockThreshold":{"type":"number","minimum":0,"description":"Mặc định 10"}},"required":["branchId"]}
            """
        ),
        Definition(
            EquipmentUtilization,
            "Mức sử dụng thiết bị của một chi nhánh trong khoảng ngày địa phương và số thiết bị đang dùng.",
            """
            {"type":"object","properties":{"branchId":{"type":"integer"},"from":{"type":"string","description":"Ngày bắt đầu dạng YYYY-MM-DD"},"to":{"type":"string","description":"Ngày kết thúc dạng YYYY-MM-DD"}},"required":["branchId","from","to"]}
            """
        ),
    ];

    public async Task<JsonElement> ExecuteAsync(
        string toolName,
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        object result = toolName switch
        {
            RevenueSummary => await RevenueAsync(arguments, context, cancellationToken),
            OrderStatistics => await OrdersAsync(arguments, context, cancellationToken),
            TopServices => await ServicesAsync(arguments, context, cancellationToken),
            InventoryAlerts => await InventoryAsync(arguments, context, cancellationToken),
            EquipmentUtilization => await EquipmentAsync(
                arguments,
                context,
                cancellationToken
            ),
            _ => throw new AiToolValidationException($"Tool '{toolName}' is not allowed."),
        };

        return JsonSerializer.SerializeToElement(result, JsonOptions);
    }

    private async Task<RevenueSummaryResult> RevenueAsync(
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        GetRevenueSummaryInput input = Parse<GetRevenueSummaryInput>(arguments);
        Validate(input.BranchId, input.From, input.To, context);
        return await dataService.GetRevenueSummaryAsync(input, cancellationToken);
    }

    private async Task<OrderStatisticsResult> OrdersAsync(
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        GetOrderStatisticsInput input = Parse<GetOrderStatisticsInput>(arguments);
        Validate(input.BranchId, input.From, input.To, context);
        return await dataService.GetOrderStatisticsAsync(
            input,
            context.TimeZone,
            cancellationToken
        );
    }

    private async Task<TopServicesResult> ServicesAsync(
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        GetTopServicesInput input = Parse<GetTopServicesInput>(arguments);
        Validate(input.BranchId, input.From, input.To, context);
        if (input.Limit is < 1 or > 10)
            throw new AiToolValidationException("limit must be between 1 and 10.");
        return await dataService.GetTopServicesAsync(input, cancellationToken);
    }

    private async Task<InventoryAlertsResult> InventoryAsync(
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        GetInventoryAlertsInput input = Parse<GetInventoryAlertsInput>(arguments);
        ValidateBranch(input.BranchId, context);
        if (input.LowStockThreshold is < 0 or > 1_000_000)
            throw new AiToolValidationException(
                "lowStockThreshold must be between 0 and 1000000."
            );
        return await dataService.GetInventoryAlertsAsync(input, cancellationToken);
    }

    private async Task<EquipmentUtilizationResult> EquipmentAsync(
        JsonElement arguments,
        AiToolExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        GetEquipmentUtilizationInput input = Parse<GetEquipmentUtilizationInput>(arguments);
        Validate(input.BranchId, input.From, input.To, context);
        return await dataService.GetEquipmentUtilizationAsync(
            input,
            context.TimeZone,
            cancellationToken
        );
    }

    private static T Parse<T>(JsonElement arguments)
    {
        if (arguments.ValueKind != JsonValueKind.Object)
            throw new AiToolValidationException("Tool arguments must be a JSON object.");

        try
        {
            return arguments.Deserialize<T>(JsonOptions)
                ?? throw new AiToolValidationException("Tool arguments are required.");
        }
        catch (JsonException)
        {
            throw new AiToolValidationException("Tool arguments are invalid.");
        }
    }

    private static void Validate(
        long branchId,
        DateOnly from,
        DateOnly to,
        AiToolExecutionContext context
    )
    {
        ValidateBranch(branchId, context);
        if (from == default || to == default || to < from)
            throw new AiToolValidationException("from and to must be valid ordered dates.");
        if (to.DayNumber - from.DayNumber + 1 > MaximumDateRangeDays)
            throw new AiToolValidationException(
                $"Date range cannot exceed {MaximumDateRangeDays} days."
            );
    }

    private static void ValidateBranch(long branchId, AiToolExecutionContext context)
    {
        if (branchId <= 0)
            throw new AiToolValidationException("branchId must be positive.");
        if (!context.AuthorizedBranchIds.Contains(branchId))
            throw new AiToolForbiddenException("The requested branch is not authorized.");
    }

    private static AiToolDefinition Definition(string name, string description, string schema)
    {
        using JsonDocument document = JsonDocument.Parse(schema);
        return new AiToolDefinition(name, description, document.RootElement.Clone());
    }
}
