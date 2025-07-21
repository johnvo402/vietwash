using Application.Common.Security;
using Contracts.Application.Common;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;

namespace Application.Feature.Common.Projections.Equipments
{
    public class EquipmentProjection : BaseResponse
    {
        public long BranchId { get; set; }
        public string Name { get; set; } = default!;

        [File]
        public string? Image { get; set; }

        public string? Description { get; set; }
        public string Code { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal Capacity { get; set; }
        public EquipmentStatus Status { get; set; }

        public virtual void MappingFrom(Equipment equipment)
        {
            Id = equipment.Id;
            PublicId = equipment.PublicId;
            CreatedAt = equipment.CreatedAt;
            CreatedBy = equipment.CreatedBy;
            UpdatedAt = equipment.UpdatedAt;
            UpdatedBy = equipment.UpdatedBy;

            BranchId = equipment.BranchId;
            Name = equipment.Name;
            Description = equipment.Description;
            Code = equipment.Code;
            Price = equipment.Price;
            Capacity = equipment.Capacity;
            Status = equipment.Status;
            Image = equipment.Image;
        }
    }
}
