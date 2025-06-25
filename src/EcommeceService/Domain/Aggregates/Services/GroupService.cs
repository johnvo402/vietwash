using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
    public class GroupService : DefaultEntity
    {
        public long ServiceId { get; set; }
        public long GroupId { get; set; }
        public long UnitRelationId { get; set; }

        public Service Service { get; set; } = default!;
        public Group Group { get; set; } = default!;
        public UnitRelation UnitRelation { get; set; } = default!;
    }
}
