using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Statistics.Queries.BranchNetRevenue
{
    public class GetNetRevenueBranchResponse
    {
        public long BranchId { get; set; }

        public decimal TotalNetRevenue { get; set; }

        public float Percentage { get; set; }
    }
}
