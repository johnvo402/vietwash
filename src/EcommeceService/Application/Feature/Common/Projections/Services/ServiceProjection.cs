using Application.Common.Security;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Enums;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public TypeStatus Type { get; set; } = default!;

        [File]
        public string? Image { get; set; }
        public ServiceStatus? Status { get; set; }

        public virtual void MappingFrom(Service service)
        {
            Name = service.Name;
            Type = service.Type;
            Image = service.Image;
            Status = service.Status;
        }
    }
}
