using Application.Common.Security;
using Domain.Aggregates.Services.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public TypeStatus Type { get; set; } = default!;
        [File]
        public string? Image { get; set; }
        public ServiceStatus? Status { get; set; }
    }

}
