using Application.Common.Security;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Enums;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public TypeStatus Type { get; set; } = default!;
        public long? CategoryId { get; set; }

        [File]
        public string? Image { get; set; }
        public ActivationStatus? Status { get; set; }

        public virtual void MappingFrom(Service service)
        {
            Id = service.Id;
            PublicId = service.PublicId;
            CreatedAt = service.CreatedAt;
            CreatedBy = service.CreatedBy;
            UpdatedAt = service.UpdatedAt;
            UpdatedBy = service.UpdatedBy;

            Name = service.Name;
            Type = service.Type;
            Image = service.Image;
            Status = service.Status;
            CategoryId = service.CategoryId;
        }
    }
}
