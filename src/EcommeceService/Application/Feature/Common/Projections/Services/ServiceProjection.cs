using Application.Common.Security;
using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceProjection : BaseEntity
    {

        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        [File]
        public string? Image { get; set; }
        public Ulid CategoryId { get; set; } = default!;
    }
}
