using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Warehouses
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int ReorderLevel { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;

        //public ICollection<InventoryRequest> InventoryRequests { get; set; } = [];
        //public ICollection<InventoryDocument> InventoryDocuments { get; set; } = [];
        public void Update(
            string? name,
            string? code,
            string? description,
            ActivationStatus? status,
            int? reorderLevel = 1
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (!string.IsNullOrWhiteSpace(code))
                Code = code;

            if (!string.IsNullOrWhiteSpace(description))
                Description = description;

            if (reorderLevel.HasValue)
                ReorderLevel = reorderLevel.Value;

            if (status.HasValue)
                Status = status.Value;
        }
    }
}
