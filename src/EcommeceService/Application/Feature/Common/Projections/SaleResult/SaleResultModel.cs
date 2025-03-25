using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.SaleResult
{
    public class SaleResultModel
    {
        public int NumberOrder { get; set; }
        public decimal Revenue { get; set; }
s        public decimal NetRevenue { get; set; }
        public decimal RevenueYesterday { get; set; }
        public float PercentageChangeDay { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public float PercentageChangeMonth { get; set; }
    }
}
