using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Common.Models.EInvoices
{
    public class ReceiptModel
    {
        public string InvoiceSymbol { get; set; } // Ký hiệu hóa đơn
        public string InvoiceNumber { get; set; } // Số hóa đơn
        public DateTime OrderDate { get; set; } // Ngày phát hành
        public string LookupCode { get; set; } // Mã tra cứu

        public OrgInfoModel OrgInfo { get; set; } // Thông tin tổ chức
        public string CustomerName { get; set; } = null!;

        public string? CustomerEmail { get; set; }

        public string? CustomerPhone { get; set; }
        public string? CustomerTaxCode { get; set; }
        public List<ReceiptItemModel> OrderItems { get; set; } = new(); // Danh sách hàng hóa

        public string Total { get; set; } // Cộng tiền hàng
        public int VatPercent { get; set; } // Thuế suất GTGT (vd: 10)
        public string TaxTotal { get; set; } // Tiền thuế GTGT
        public string Discount { get; set; } // Tiền thuế GTGT

        public string TotalWithTax { get; set; } // Tổng cộng thanh toán
        public string TotalInWords { get; set; } // Số tiền viết bằng chữ

        public string QrCodeUrl { get; set; } // Link ảnh QR code
    }

    public class OrgInfoModel
    {
        public string Name { get; set; } // Tên đơn vị bán hàng
        public string TaxCode { get; set; } // Mã số thuế
        public string Address { get; set; } // Địa chỉ
        public string Phone { get; set; } // Số điện thoại
        public string? Logo { get; set; } // Link ảnh logo
        public string? Stamp { get; set; } // Link ảnh con dấu
    }

    public class ReceiptItemModel
    {
        public string ServiceName { get; set; }
        public string UnitRelationName { get; set; }
        public int Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? TotalPriceItem { get; set; }
    }
}
