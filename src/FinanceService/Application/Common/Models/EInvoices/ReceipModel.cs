using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Common.Models.EInvoices
{
    public class ReceiptModel
    {
        public string InvoiceSymbol { get; set; } = default!; // Ký hiệu hóa đơn
        public string InvoiceNumber { get; set; } = default!; // Số hóa đơn
        public DateTime OrderDate { get; set; } // Ngày phát hành
        public string LookupCode { get; set; } = default!; // Mã tra cứu

        public OrgInfoModel OrgInfo { get; set; } = default!; // Thông tin tổ chức
        public string CustomerName { get; set; } = null!;

        public string? CustomerEmail { get; set; }

        public string? CustomerPhone { get; set; }
        public string? CustomerTaxCode { get; set; }
        public List<ReceiptItemModel> OrderItems { get; set; } = new(); // Danh sách hàng hóa

        public string Total { get; set; } = default!; // Cộng tiền hàng
        public int VatPercent { get; set; } // Thuế suất GTGT (vd: 10)
        public string TaxTotal { get; set; } = default!; // Tiền thuế GTGT
        public string Discount { get; set; } = default!; // Tiền thuế GTGT

        public string TotalWithTax { get; set; } = default!; // Tổng cộng thanh toán
        public string TotalInWords { get; set; } = default!; // Số tiền viết bằng chữ

        public string QrCodeUrl { get; set; } = default!; // Link ảnh QR code
    }

    public class OrgInfoModel
    {
        public string Name { get; set; } = default!; // Tên đơn vị bán hàng
        public string TaxCode { get; set; } = default!; // Mã số thuế
        public string Address { get; set; } = default!; // Địa chỉ
        public string Phone { get; set; } = default!; // Số điện thoại
        public string? Logo { get; set; } // Link ảnh logo
        public string? Stamp { get; set; } // Link ảnh con dấu
    }

    public class ReceiptItemModel
    {
        public string ServiceName { get; set; } = default!;
        public string UnitRelationName { get; set; } = default!;
        public int Quantity { get; set; }
        public string? UnitPrice { get; set; }
        public string? TotalPriceItem { get; set; }
    }
}
