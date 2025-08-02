using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Reports.ProductSupplierReport
{
    public class ProductSupplierReportResponse
    {
        public long SupplierId { get; set; }
        public long BranchId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int SupplierProductTypeCount { get; set; }
        public decimal ImportedValueTotal { get; set; }
        public decimal ExportedValueTotal { get; set; }
    }
}
