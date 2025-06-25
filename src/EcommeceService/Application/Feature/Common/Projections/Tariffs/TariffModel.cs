using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Tariffs
{
    public class TariffModel
    {
        public string Name { get; set; }
        public bool Disable { get; set; }
        public long BranchId { get; set; }
    }
}
