using Domain.Aggregates.Branches.Enums;
using JohnChum.SharedKernel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Branches
{
    public class BranchProduct : DefaultEntity<long>
    {
        public long BranchId { get; set; }
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public BranchStatus Status { get; set; }
        public string? Barcode { get; set; }
        public string? ImgUrl { get; set; }
        public virtual Branch Branch { get; set; } = default!;


    }
}