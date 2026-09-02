"use client";

import { formatOrderMoney as formatPriceVN } from "@/utils/format";
import { forwardRef, useImperativeHandle, useRef } from "react";
import { useAuth } from "@/hooks/use-auth";
import { useTranslations } from "next-intl";
import { Order } from "@/features/cashier/types";

interface PickupTicketProps {
  order: Order;
}

export interface PickupTicketRef {
  print: () => void;
}

const PickupTicket = forwardRef<PickupTicketRef, PickupTicketProps>(
  ({ order }, ref) => {
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
        <title>${t("order.appointmentSlip")}</title>
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
              <img
                style={{ width: "25%" }}
                src="/logo/favicon.svg"
                alt="Logo"
              />
            </div>

            <h1
              style={{
                fontSize: "18px",
                fontWeight: "bold",
                textAlign: "center",
              }}
            >
              {t("order.appointmentSlip")}
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
                (branch) => branch.branchId === order.branchId,
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
                  width: "50%",
                }}
              >
                <h2
                  style={{ fontSize: "16px", borderBottom: "1px solid #000" }}
                >
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
                <p>
                  <strong>{t("order.expectedDelivery")}:</strong>{" "}
                  {order.deliveryTime
                    ? new Date(order.deliveryTime).toLocaleString()
                    : "--"}
                </p>
              </div>
              <div
                style={{
                  width: "50%",
                }}
              >
                <h2
                  style={{ fontSize: "16px", borderBottom: "1px solid #000" }}
                >
                  {t("user.staffInformation")}
                </h2>
                <p>
                  <strong>{t("table.accessorKey.name")}:</strong>{" "}
                  {order.staff?.displayName || ""}
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
                {order.discountAmount !== undefined
                  ? formatPriceVN(order.discountAmount)
                  : order.discountFixed
                    ? formatPriceVN(order.discountValue)
                    : `${order.discountValue}%`}
              </span>
            </div>
            <div className="summary-row">
              <span style={{ fontWeight: "bold" }}>VAT({order.vat}%):</span>
              <span style={{ fontWeight: "bold" }}>
                {formatPriceVN(order.vatAmount ?? 0)}
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
              marginTop: "10mm",
            }}
          >
            <div style={{ textAlign: "center" }}>
              <p>
                <strong>Mã xác nhận</strong>
              </p>
              <p>
                <img src={order.qrCode} alt="Logo" width="200mm" />
              </p>
            </div>
            <div style={{ marginTop: "10mm", textAlign: "center" }}>
              <p>
                <strong>Cửa hàng xác nhận</strong>
              </p>
              <img
                src="data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPCFET0NUWVBFIHN2ZyBQVUJMSUMgIi0vL1czQy8vRFREIFNWRyAxLjEvL0VOIiAiaHR0cDovL3d3dy53My5vcmcvR3JhcGhpY3MvU1ZHLzEuMS9EVEQvc3ZnMTEuZHRkIj4KPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHN0eWxlPSJiYWNrZ3JvdW5kOiB0cmFuc3BhcmVudDsgYmFja2dyb3VuZC1jb2xvcjogdHJhbnNwYXJlbnQ7IGNvbG9yLXNjaGVtZTogbGlnaHQgZGFyazsiIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB2ZXJzaW9uPSIxLjEiIHdpZHRoPSIxOTRweCIgaGVpZ2h0PSI2M3B4IiB2aWV3Qm94PSItMC41IC0wLjUgMTk0IDYzIj48ZGVmcy8+PGc+PGcgZGF0YS1jZWxsLWlkPSIwIj48ZyBkYXRhLWNlbGwtaWQ9IjEiPjxnIGRhdGEtY2VsbC1pZD0iRHQzZFZDSW44YWNPdGJ3QmgzMXItMSI+PGc+PHJlY3QgeD0iMSIgeT0iMSIgd2lkdGg9IjE5MCIgaGVpZ2h0PSI2MCIgZmlsbC1vcGFjaXR5PSIwLjUiIGZpbGw9IiNmZmZmZmYiIHN0cm9rZT0iI2ZmMzMzMyIgc3Ryb2tlLW9wYWNpdHk9IjAuNSIgc3Ryb2tlLXdpZHRoPSIzIiBwb2ludGVyLWV2ZW50cz0iYWxsIiBzdHlsZT0iZmlsbDogbGlnaHQtZGFyaygjZmZmZmZmLCB2YXIoLS1nZS1kYXJrLWNvbG9yLCAjMTIxMjEyKSk7IHN0cm9rZTogbGlnaHQtZGFyayhyZ2IoMjU1LCA1MSwgNTEpLCByZ2IoMjU1LCAxMTksIDExOSkpOyIvPjwvZz48L2c+PGcgZGF0YS1jZWxsLWlkPSJEdDNkVkNJbjhhY090YndCaDMxci01Ij48Zz48cmVjdCB4PSIxMSIgeT0iMTEiIHdpZHRoPSIxNzAiIGhlaWdodD0iNDAiIGZpbGwtb3BhY2l0eT0iMC41IiBmaWxsPSIjZmZmZmZmIiBzdHJva2U9IiNmZjAwMDAiIHN0cm9rZS1vcGFjaXR5PSIwLjUiIHBvaW50ZXItZXZlbnRzPSJhbGwiIHN0eWxlPSJmaWxsOiBsaWdodC1kYXJrKCNmZmZmZmYsIHZhcigtLWdlLWRhcmstY29sb3IsICMxMjEyMTIpKTsgc3Ryb2tlOiBsaWdodC1kYXJrKHJnYigyNTUsIDAsIDApLCByZ2IoMjU1LCAxNDQsIDE0NCkpOyIvPjwvZz48L2c+PGcgZGF0YS1jZWxsLWlkPSJEdDNkVkNJbjhhY090YndCaDMxci0yIj48Zz48cmVjdCB4PSIxMSIgeT0iMTYiIHdpZHRoPSIxMjAiIGhlaWdodD0iMzAiIGZpbGw9Im5vbmUiIHN0cm9rZT0ibm9uZSIgcG9pbnRlci1ldmVudHM9ImFsbCIvPjwvZz48Zz48ZyBmaWxsPSIjRkYwMDAwIiBmb250LWZhbWlseT0iJnF1b3Q7SGVsdmV0aWNhJnF1b3Q7IiBmb250LXNpemU9IjEycHgiIG9wYWNpdHk9IjAuNiIgc3R5bGU9ImZpbGw6IGxpZ2h0LWRhcmsocmdiKDI1NSwgMCwgMCksIHJnYigyNTUsIDE0NCwgMTQ0KSk7Ij48dGV4dCB4PSIxMi41IiB5PSIyOC41Ij5D4butYSBIw6BuZyBHaeG6t3TCoDwvdGV4dD48dGV4dCB4PSIxMi41IiB5PSI0Mi41Ij7hu6ZpIC0gVmlldFdhc2g8L3RleHQ+PC9nPjwvZz48L2c+PGcgZGF0YS1jZWxsLWlkPSJEdDNkVkNJbjhhY090YndCaDMxci00Ij48Zz48Zz48c3ZnIHZlcnNpb249IjEuMCIgd2lkdGg9IjY5LjY3IiBoZWlnaHQ9IjIwIiB2aWV3Qm94PSItMC4wMDAwMDExNDQ0MDkyMjUxNjIyMzUgMCAyMDguODYxNzA5NTk0NzI2NTYgNTkuNzMwMTYzNTc0MjE4NzUiIHByZXNlcnZlQXNwZWN0UmF0aW89InhNaWRZTWlkIiBpZD0ic3ZnMTEiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyIgeD0iMTAwLjUiIHk9IjIwLjUiIHN0eWxlPSJmb250LWZhbWlseTogaW5pdGlhbDsiPiYjeGE7ICA8ZGVmcyBpZD0iZGVmczExIi8+JiN4YTsgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuMSwwLDAsLTAuMSwtMjQuMTM4Mjk2LDg0Ljk3ODg4KSIgZmlsbD0iIzAwMDAwMCIgc3Ryb2tlPSJub25lIiBpZD0iZzExIiBzdHlsZT0iZmlsbDojZmYwMDAwIj4mI3hhOyAgICA8cGF0aCBkPSJtIDEyMjQsNTkwIGMgMCwtMTg0IDIsLTI2MCAzLC0xNjcgMiw5MiAyLDI0MiAwLDMzNSAtMSw5MiAtMywxNiAtMywtMTY4IHoiIGlkPSJwYXRoMSIgc3R5bGU9ImZpbGw6I2ZmMDAwMCIvPiYjeGE7ICAgIDxwYXRoIGQ9Im0gMTc1NSw4MzAgYyAtOCwtMjYgNiwtNDMgMzEsLTM4IDEzLDIgMTksMTIgMTksMjggMCwzNCAtNDAsNDEgLTUwLDEwIHoiIGlkPSJwYXRoMiIgc3R5bGU9ImZpbGw6I2ZmMDAwMCIvPiYjeGE7ICAgIDxwYXRoIGQ9Im0gOTk4LDc4OCAtNjYsLTM5IDUsLTUyIEMgOTUyLDU0MSA3OTgsMzk0IDYzOCw0MTMgYyAtOTQsMTIgLTE4Niw4MSAtMjIzLDE3MSAtMTYsMzggLTIxLDE1MSAtNywxNjAgNSwzIDM4LC01NSA3MywtMTI5IDU5LC0xMjQgNjcsLTEzNSAxMDIsLTE1MCAzOCwtMTUgMTQyLC0yMSAxNTAsLTcgMiw0IC0zMyw4NiAtNzksMTgyIGwgLTgzLDE3NSAtMTU1LDMgLTE1NSwzIC0xMiwtNDMgYyAtNiwtMjQgLTksLTgxIC03LC0xMjggMywtNzAgMTAsLTk4IDM2LC0xNTMgNTgsLTEyNCAxODIsLTIxNSAzMjYsLTIzOSAyNjMsLTQ0IDUxNSwxODIgNDkzLDQ0MiAtMywzNyAtMTIsODAgLTE5LDk3IGwgLTEzLDMwIHoiIGlkPSJwYXRoMyIgc3R5bGU9ImZpbGw6I2ZmMDAwMCIvPiYjeGE7ICAgIDxwYXRoIGQ9Im0gMTQ4NSw4MDEgYyAtNywtMTMgNTMsLTE3NyA3MCwtMTkxIDgsLTYgMjQsLTEwIDM3LC04IDI5LDQgNzgsOTggODYsMTY0IDUsNDUgNSw0NiAtMjQsNDIgLTI2LC0zIC0yOSwtOSAtNDUsLTcxIC05LC0zOCAtMTksLTcwIC0yMSwtNzMgLTMsLTIgLTEzLDI0IC0yMyw1OCAtMTAsMzUgLTIzLDY5IC0yOCw3NiAtMTEsMTQgLTQzLDE2IC01MiwzIHoiIGlkPSJwYXRoNCIgc3R5bGU9ImZpbGw6I2ZmMDAwMCIvPiYjeGE7ICAgIDxwYXRoIGQ9Im0gMjEyNyw4MDQgYyAtNCwtNCAtNywtNDMgLTcsLTg2IDAsLTY1IDMsLTgxIDIwLC05OCAyMSwtMjEgNTUsLTI2IDc5LC0xMSAxOCwxMiA5LDM2IC0xNSwzNiAtMTQsMCAtMjAsOCAtMjIsMzIgLTMsMjYgMSwzMiAyMCwzNSAxNSwyIDI0LDExIDI2LDI2IDMsMTggLTEsMjIgLTIyLDIyIC0xOCwwIC0yNSw2IC0yOCwyMyAtMywyMiAtMzYsMzYgLTUxLDIxIHoiIGlkPSJwYXRoNSIgc3R5bGU9ImZpbGw6I2ZmMDAwMCIvPiYjeGE7ICAgIDxwYXRoIGQ9Im0gMTc1MCw2OTYgYyAwLC03NyAxMywtMTA0IDQ1LC05MiAxMiw0IDE1LDIzIDE1LDg2IHYgODAgaCAtMzAgLTMwIHoiIGlkPSJwYXRoNiIgc3R5bGU9ImZpbGw6I2ZmMDAwMCIvPiYjeGE7ICAgIDxwYXRoIGQ9Im0gMTkxNyw3NTggYyAtMzcsLTI5IC00MiwtOTMgLTksLTEzMCAzMiwtMzggMTMyLC0zNSAxMzIsMyAwLDE5IC00LDIxIC0zMSwxNSAtMTcsLTMgLTQwLC0xIC01Miw2IC0yMCwxMSAtMTcsMTMgMzMsMjMgNDUsOCA1NiwxNCA1OCwzMiA3LDUwIC04NSw4NSAtMTMxLDUxIHogbSA3MywtMzcgYyAwLC01IC0xMSwtMTEgLTI1LC0xMyAtMjQsLTUgLTMyLDIgLTE4LDE1IDksMTAgNDMsOCA0MywtMiB6IiBpZD0icGF0aDciIHN0eWxlPSJmaWxsOiNmZjAwMDAiLz4mI3hhOyAgICA8cGF0aCBkPSJNIDIxNzAsNDUwIFYgMzQwIGggMjUgYyAyNCwwIDI1LDMgMjUsNjAgMCwzMyA1LDYxIDEwLDYyIDMzLDQgMzUsMSA0MCwtNTcgNSwtNTcgNywtNjAgMzMsLTYzIGwgMjcsLTMgdiA2NSBjIDAsNzkgLTE5LDEwNiAtNzUsMTA2IC0zMSwwIC0zNSwzIC0zNSwyNSAwLDIwIC01LDI1IC0yNSwyNSBoIC0yNSB6IiBpZD0icGF0aDgiIHN0eWxlPSJmaWxsOiNmZjAwMDAiLz4mI3hhOyAgICA8cGF0aCBkPSJtIDEzOTEsNTQxIGMgLTE1LC0xMCAtMTAsLTU5IDE1LC0xNDggMTQsLTUyIDE1LC01MyA1NCwtNTMgMzcsMCAzOSwyIDQ2LDM1IDMsMTkgMTAsMzUgMTQsMzUgNCwwIDExLC0xNiAxNCwtMzUgNywtMzMgOSwtMzUgNDYsLTM1IDM3LDAgNDAsMiA1MCwzOCA2LDIwIDEzLDY0IDE3LDk4IDUsNTMgMyw2MiAtMTMsNjggLTI1LDEwIC00NCwtMTggLTQ0LC02MyAwLC0yMCAtMywtNDcgLTcsLTYxIC02LC0yMiAtOCwtMTkgLTIyLDI1IC0xMyw0MiAtMjAsNTAgLTQzLDUzIC0yMywzIC0yOCwtMiAtMzcsLTM1IC02LC0yMSAtMTEsLTQ1IC0xMSwtNTMgLTEsLTMyIC0xNywxNSAtMjQsNzAgLTgsNjMgLTI0LDgxIC01NSw2MSB6IiBpZD0icGF0aDkiIHN0eWxlPSJmaWxsOiNmZjAwMDAiLz4mI3hhOyAgICA8cGF0aCBkPSJtIDE3NDMsNTAzIGMgLTcsLTIgLTEzLC0xNCAtMTMsLTI1IDAsLTE3IDUsLTIwIDMwLC0xNSAzMiw3IDYwLDAgNjAsLTE0IDAsLTUgLTExLC05IC0yNCwtOSAtNDksMCAtNzYsLTE3IC03NiwtNDkgMCwtMzkgMjQsLTU2IDgyLC01NiA2NCwwIDgxLDIwIDc1LDg5IC01LDYwIC0zMiw4NiAtODYsODUgLTIwLDAgLTQyLC0zIC00OCwtNiB6IG0gNjksLTEyNSBjIC0xMiwtMTIgLTQyLC00IC00MiwxMSAwLDEyIDQ0LDE5IDQ5LDggMiwtNSAtMSwtMTMgLTcsLTE5IHoiIGlkPSJwYXRoMTAiIHN0eWxlPSJmaWxsOiNmZjAwMDAiLz4mI3hhOyAgICA8cGF0aCBkPSJtIDE5NzIsNDk0IGMgLTI0LC0xNyAtMjksLTU2IC05LC03MiA2LC02IDI1LC0xNCA0MCwtMTcgNDYsLTEyIDQzLC0yNyAtNCwtMjQgLTM4LDMgLTQ0LDEgLTQ0LC0xNiAwLC0xNSAxMCwtMjIgMzgsLTMwIDUzLC0xMyA5Miw5IDkyLDUwIDAsMjUgLTcsMzMgLTM4LDQ4IC00MiwyMSAtNDEsMzkgMiwzMSAzNCwtNyA0Nyw4IDI1LDMwIC0yMCwyMCAtNzMsMjEgLTEwMiwwIHoiIGlkPSJwYXRoMTEiIHN0eWxlPSJmaWxsOiNmZjAwMDAiLz4mI3hhOyAgPC9nPiYjeGE7PC9zdmc+PC9nPjwvZz48L2c+PC9nPjwvZz48L2c+PC9zdmc+"
                alt="Logo"
                width="150mm"
              />
            </div>
          </div>
        </div>
      </div>
    );
  },
);

PickupTicket.displayName = "PickupTicket";
export default PickupTicket;
