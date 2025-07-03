using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Warehouses;
using Shared.Kernel.Common;

namespace Application.Features.Common.Projections.Warehouses
{
    public class WarehouseProjection : BaseEntity<long>
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int ReorderLevel { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public ActivationStatus Status { get; set; }

        public virtual void MappingFrom(Warehouse entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Code = entity.Code;
            Description = entity.Description;
            ReorderLevel = entity.ReorderLevel;
            BranchId = entity.BranchId;
            Status = entity.Status;

            CreatedAt = entity.CreatedAt;
            CreatedBy = entity.CreatedBy;
            UpdatedAt = entity.UpdatedAt;
            UpdatedBy = entity.UpdatedBy;
        }
    }
}
