using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Services
{
    public class GroupService : DefaultEntity
    {
        public Ulid ServiceId { get; set; }
        public Ulid GroupId { get; set; }
        public Ulid UnitRelationId { get; set; }

        public Service Service { get; set; } = default!;
        public Group Group { get; set; } = default!;
        public UnitRelation UnitRelation { get; set; } = default!;
    }
}
