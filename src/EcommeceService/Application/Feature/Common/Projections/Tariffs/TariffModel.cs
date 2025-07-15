using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.Tariffs;

public class TariffModel
{
    public long BranchId { get; set; }
    public string Name { get; set; }
    public ActivationStatus Status { get; set; } = ActivationStatus.Active;
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
}
