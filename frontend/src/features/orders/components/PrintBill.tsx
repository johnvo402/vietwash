"use client";

import { formatPriceVN } from "@/utils/format";
import { forwardRef, useImperativeHandle, useRef } from "react";
import { useAuth } from "@/hooks/use-auth";
import { useTranslations } from "next-intl";
import { Order } from "@/features/cashier/types";

interface PrintBillProps {
  order: Order;
}

export interface PrintBillRef {
  print: () => void;
}

const PrintBill = forwardRef<PrintBillRef, PrintBillProps>(({ order }, ref) => {
  const ticketRef = useRef<HTMLDivElement>(null);
  const { user } = useAuth();
  const t = useTranslations();

  const handlePrint = () => {
    if (!ticketRef.current) return;

    const content = ticketRef.current.innerHTML;

    const iframe = document.createElement("iframe");
    iframe.style.position = "fixed";
    iframe.style.right = "0";
    iframe.style.bottom = "0";
    iframe.style.width = "0";
    iframe.style.height = "0";
    iframe.style.border = "0";
    document.body.appendChild(iframe);

    const doc = iframe.contentWindow?.document;
    if (!doc) return;

    doc.open();
    doc.write(`
    <html>
      <head>
        <title>${t("order.billTitle")}</title>
        <style>
          @media print {
            @page { size: auto; margin: 10mm; }
            body { font-family: Arial, sans-serif; font-size: 12px; padding: 0; margin: 0; }
            table { width: 100%; border-collapse: collapse; }
            th, td { border: 1px solid #000; padding: 6px; text-align: left; }
            h1, h2, p { margin: 4px 0; }
            .summary-row {font-size: 20px; display: flex; justify-content: space-between; margin-top: 6px; }
          }
        </style>
      </head>
      <body>
        ${content}
        <script>
          window.onload = function() {
            window.focus();
            window.print();
            setTimeout(() => { window.close(); }, 100);
          }
        </script>
      </body>
    </html>
  `);
    doc.close();
  };

  useImperativeHandle(ref, () => ({
    print: handlePrint,
  }));

  return (
    <div style={{ display: "none" }}>
      <div ref={ticketRef}>
        <div style={{ width: "100%", boxSizing: "border-box" }}>
          <div
            style={{
              display: "flex",
              justifyContent: "center",
              alignItems: "center",
            }}
          >
            <img style={{ width: "25%" }} src="/logo/favicon.svg" alt="Logo" />
          </div>

          <h1
            style={{
              fontSize: "18px",
              fontWeight: "bold",
              textAlign: "center",
            }}
          >
            {t("order.billTitle")}
          </h1>

          <p style={{ textAlign: "center" }}>
            <strong>{t("table.accessorKey.code")}:</strong> {order.code}
          </p>
          <p style={{ textAlign: "center" }}>
            <strong>
              {t("common.branch").charAt(0).toUpperCase() +
                t("common.branch").slice(1)}
              :
            </strong>{" "}
            {user?.branchAccounts.find(
              (branch) => branch.branchId === order.branchId
            )?.branchName ?? "--"}
          </p>
          <div
            style={{
              width: "100%",
              display: "flex",
              justifyContent: "space-between",
            }}
          >
            <div
              style={{
                width: "100%",
              }}
            >
              <h2 style={{ fontSize: "16px", borderBottom: "1px solid #000" }}>
                {t("user.customerInformation")}
              </h2>
              <p>
                <strong>{t("table.accessorKey.name")}:</strong>{" "}
                {order.customer?.displayName || ""}
              </p>
              <p>
                <strong>{t("user.phoneNumber.title")}:</strong>{" "}
                {order.customer?.phoneNumber || ""}
              </p>
              <p>
                <strong>{t("order.orderDate")}:</strong>{" "}
                {order.createdAt
                  ? new Date(order.createdAt).toLocaleString()
                  : "--"}
              </p>
            </div>
          </div>

          <h2 style={{ fontSize: "16px" }}>{t("common.details")}</h2>
          <table>
            <thead>
              <tr>
                <th style={{ textAlign: "center" }}>
                  {t("table.accessorKey.index")}
                </th>
                <th style={{ textAlign: "center" }}>
                  {t("common.service").charAt(0).toUpperCase() +
                    t("common.service").slice(1)}
                </th>
                <th style={{ textAlign: "center" }}>
                  {t("table.accessorKey.quantity")}
                </th>
                <th style={{ textAlign: "center" }}>{t("product.unit")}</th>
                <th style={{ textAlign: "center" }}>
                  {t("table.accessorKey.amount")}
                </th>
              </tr>
            </thead>
            <tbody>
              {order.orderItems?.map((item, index) => (
                <tr key={index}>
                  <td style={{ textAlign: "center" }}>{index + 1}</td>
                  <td>
                    {item.serviceName} ({formatPriceVN(item.unitPrice ?? 0)} /{" "}
                    {item.unitRelationName})
                  </td>
                  <td style={{ textAlign: "right" }}>{item.quantity}</td>
                  <td>{item.unitRelationName}</td>
                  <td style={{ textAlign: "right" }}>
                    {formatPriceVN(item.price ?? 0)}
                  </td>
                </tr>
              ))}
              <tr>
                <td
                  colSpan={3}
                  style={{ fontWeight: "bold", textAlign: "right" }}
                >
                  {t("table.accessorKey.total").toUpperCase()}:
                </td>
                <td
                  colSpan={2}
                  style={{ fontWeight: "bold", textAlign: "right" }}
                >
                  {formatPriceVN(order.amount ?? 0)}
                </td>
              </tr>
            </tbody>
          </table>
          <div className="summary-row">
            <span style={{ fontWeight: "bold" }}>{t("order.discount")}:</span>
            <span style={{ fontWeight: "bold" }}>
              {order.discountFixed
                ? formatPriceVN(order.discountValue)
                : `${order.discountValue}%`}
            </span>
          </div>
          <div className="summary-row">
            <span style={{ fontWeight: "bold" }}>VAT({order.vat}%):</span>
            <span style={{ fontWeight: "bold" }}>
              {order.vatAmount ? formatPriceVN(order.vatAmount) : "--"}
            </span>
          </div>
          <div className="summary-row">
            <span style={{ fontWeight: "bold" }}>
              {t("table.accessorKey.thanhtien")}:
            </span>
            <span style={{ fontWeight: "bold" }}>
              {formatPriceVN(order.total ?? 0)}
            </span>
          </div>
        </div>
        <div
          style={{
            width: "100%",
            display: "fixed",
            bottom: "0",
            marginTop: "10mm",
          }}
        >
          <p style={{ textAlign: "center", marginTop: "10px" }}>
            {t("thankYou")}
          </p>
        </div>
      </div>
    </div>
  );
});

PrintBill.displayName = "PrintBill";
export default PrintBill;
