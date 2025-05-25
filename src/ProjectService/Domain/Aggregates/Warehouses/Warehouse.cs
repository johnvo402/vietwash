using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Warehouses
{
    public class Warehouse : BaseEntity<long>
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int ReorderLevel { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public short Status { get; set; } = default!;


        //public ICollection<InventoryRequest> InventoryRequests { get; set; } = [];
        //public ICollection<InventoryDocument> InventoryDocuments { get; set; } = [];
    }
}
