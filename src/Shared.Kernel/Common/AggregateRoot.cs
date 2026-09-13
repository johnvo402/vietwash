using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Shared.Kernel.Common.Events;

namespace Shared.Kernel.Common;

public abstract class AggregateRoot : DefaultEntity, IAuditable
{
    public long Version { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonIgnore]
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => uncommittedEvents;

    [JsonIgnore]
    [NotMapped]
    private readonly Queue<IDomainEvent> uncommittedEvents = [];

    public IDomainEvent[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    public bool TryDequeueUncommittedEvent(out IDomainEvent? domainEvent) =>
        uncommittedEvents.TryDequeue(out domainEvent);

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        uncommittedEvents.Enqueue(domainEvent);
}
