using Shared.Kernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Branches
{
    public class BranchUser : DefaultEntity<long>
    {
        public long UserId { get; set; }
        public long BranchId { get; set; }
        public bool Manager { get; set; }
        public virtual Branch Branch { get; set; } = default!;

    }
}