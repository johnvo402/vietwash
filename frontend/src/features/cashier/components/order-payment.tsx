import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import {
  Loader2,
  Printer,
  ShoppingCart,
  Plus,
  QrCode,
  Save,
} from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { Customer } from "@/utils/customer-indexedDb";
import { PriceItem } from "@/utils/tariff-db";
import CustomDateTime from "./booking-receipt-date";
import { toast } from "react-toastify";
import dynamic from "next/dynamic";
import type { PreviewOrderResponse } from "@/api/generated";
import { PricingSummary } from "./pricing-summary";

const QRScanner = dynamic(
  () => import("@/components/qr-scanner").then((module) => module.QRScanner),
  { ssr: false },
);

interface OrderPaymentProps {
  preview?: PreviewOrderResponse;
  calculating: boolean;
  previewError: boolean;
  previewErrorMessage?: string;
  retryPreview: () => void;
  voucherCode: string;
  disable: boolean;
  isProcessing: boolean;
  handleProcessOrder: () => void;
  handlePrint: () => void;
  printDisabled: boolean;
  handleApplyVoucher: (code: string, tabId: string) => void;
  activeTab: string;
  customers: Customer[];
  tariffs: PriceItem[];
  tariffId: number;
  onSetTariff: (id: number) => void;
  note: string;
  onSetNote: (note: string) => void;
  customerInit: Customer | null;
  onSelect: (customer: Customer | null) => void;
  openCreate: () => void;
  deliveryTime: string;
  onSetDeliveryTime: (date: Date | undefined) => void;
  customerPending: boolean;
  isEdit: boolean;
}

export function OrderPaymentSection(props: OrderPaymentProps) {
  const t = useTranslations();
  const [voucherCode, setVoucherCode] = useState(props.voucherCode);
  const [searchTerm, setSearchTerm] = useState("");
  const [showQRScanner, setShowQRScanner] = useState(false);
  const customer = props.customerInit;
  const tariff = props.tariffs.find((row) => row.id === props.tariffId);
  useEffect(() => {
    setVoucherCode(props.voucherCode);
    setShowQRScanner(false);
  }, [props.voucherCode, props.activeTab]);
  const applyVoucher = (code: string) => {
    setVoucherCode(code);
    props.handleApplyVoucher(code, props.activeTab);
  };
  return (
    <Card className="bg-background shadow-md rounded-lg">
      <CardHeader>
        <CardTitle className="text-xl font-bold">
          {t("cashier.orderInformation")}
        </CardTitle>
      </CardHeader>
      <fieldset disabled={props.isProcessing} className="min-w-0">
        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-1">
            <div className="space-y-2">
              <Label htmlFor="customer-select">
                {t("user.customerInformation")}
              </Label>
              <Select
                value={customer?.id.toString() ?? ""}
                disabled={props.isEdit || props.customerPending}
                onValueChange={(value) =>
                  value === "create"
                    ? props.openCreate()
                    : props.onSelect(
                        props.customers.find(
                          (row) => row.id === Number(value),
                        ) ?? null,
                      )
                }
              >
                <SelectTrigger id="customer-select">
                  <SelectValue
                    placeholder={t("common.placeholderSelect", {
                      entity: t("common.customer"),
                    })}
                  />
                </SelectTrigger>
                <SelectContent onCloseAutoFocus={() => setSearchTerm("")}>
                  <Input
                    aria-label={t("cashier.customerPlaceholder")}
                    value={searchTerm}
                    onChange={(event) => setSearchTerm(event.target.value)}
                    placeholder={t("cashier.customerPlaceholder")}
                  />
                  {props.customers
                    .filter((row) =>
                      `${row.displayName} ${row.phoneNumber}`
                        .toLowerCase()
                        .includes(searchTerm.toLowerCase()),
                    )
                    .map((row) => (
                      <SelectItem key={row.id} value={row.id.toString()}>
                        {row.displayName} - {row.phoneNumber}
                      </SelectItem>
                    ))}
                  <SelectItem value="create">
                    <span className="flex items-center gap-2">
                      <Plus className="h-4 w-4" aria-hidden="true" />
                      {t("common.create")}
                    </span>
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>{t("common.tariff")}</Label>
              <Combobox
                ariaLabel={t("common.tariff")}
                options={props.tariffs.map((row) => ({
                  value: String(row.id),
                  label: row.name,
                }))}
                value={tariff ? String(tariff.id) : ""}
                onChange={(id) => {
                  if (id) props.onSetTariff(Number(id));
                }}
                placeholder={t("common.entitySelectPlaceholder", {
                  entity: t("common.tariff"),
                })}
                searchPlaceholder={t("search.searchBy", {
                  entity: t("common.tariff"),
                })}
                emptyMessage={t("common.noOptions")}
                disabled={!props.tariffs.length || props.isProcessing}
              />
            </div>
          </div>
          {props.customerPending && (
            <div role="status" className="space-y-2 text-sm">
              <p>{t("cashier.customerSyncPending")}</p>
              <Button variant="outline" onClick={props.openCreate}>
                {t("cashier.retryCustomerSync")}
              </Button>
            </div>
          )}
          <div>
            <Label htmlFor="customer-note">{t("common.note")}</Label>
            <Input
              id="customer-note"
              value={props.note}
              onChange={(event) => props.onSetNote(event.target.value)}
            />
          </div>
          <CustomDateTime
            onChange={props.onSetDeliveryTime}
            showSeconds
            date={props.deliveryTime ? new Date(props.deliveryTime) : undefined}
            placeholder={t("cashier.selectPickupTime")}
          />
          <div className="space-y-2">
            <Label htmlFor="voucher-code">
              {t("cashier.enterVoucherCode")}
            </Label>
            <div className="flex items-center gap-2">
              <Input
                id="voucher-code"
                disabled={!customer || props.isEdit}
                value={voucherCode}
                onChange={(event) => setVoucherCode(event.target.value)}
              />
              <Button
                variant="outline"
                aria-label={t("cashier.enterVoucherCode")}
                disabled={!customer || props.isEdit}
                onClick={() => setShowQRScanner(!showQRScanner)}
              >
                <QrCode className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                disabled={!customer || props.isEdit}
                onClick={() => applyVoucher(voucherCode)}
              >
                {t("common.apply")}
              </Button>
            </div>
            {showQRScanner && (
              <QRScanner
                onScanSuccess={(code) => {
                  applyVoucher(code);
                  setShowQRScanner(false);
                }}
                onScanError={(error) => toast.error(error)}
                onStop={() => setShowQRScanner(false)}
                autoStart
              />
            )}
          </div>
        </CardContent>
        <CardFooter className="p-6 bg-background border-t border-secondary">
          <div className="space-y-4 w-full">
            {props.isEdit ? (
              <p className="text-sm text-muted-foreground">
                {t("cashier.editPricingOnSave")}
              </p>
            ) : (
              <>
                <PricingSummary
                  preview={props.preview}
                  calculating={props.calculating}
                  error={props.previewError}
                  labels={{
                    amount: t("table.accessorKey.amount"),
                    discount: t("order.discount"),
                    total: t("table.accessorKey.total"),
                    calculating: t("cashier.calculating"),
                    error:
                      props.previewErrorMessage || t("cashier.previewFailed"),
                  }}
                />
                {props.previewError && (
                  <Button variant="outline" onClick={props.retryPreview}>
                    {t("cashier.retryPreview")}
                  </Button>
                )}
              </>
            )}
            <div className="w-full flex gap-2">
              <Button
                variant="outline"
                aria-label={t("order.appointmentSlip")}
                disabled={props.printDisabled || props.isEdit}
                onClick={props.handlePrint}
              >
                <Printer className="h-4 w-4" />
              </Button>
              <Button
                className="flex-1"
                disabled={props.disable || props.isProcessing}
                onClick={props.handleProcessOrder}
              >
                {props.isProcessing ? (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                ) : props.isEdit ? (
                  <Save className="mr-2 h-4 w-4" />
                ) : (
                  <ShoppingCart className="mr-2 h-4 w-4" />
                )}
                {t(
                  props.isProcessing
                    ? "common.status.handling"
                    : props.isEdit
                      ? "cashier.updateOrder"
                      : "cashier.createOrder",
                )}
              </Button>
            </div>
          </div>
        </CardFooter>
      </fieldset>
    </Card>
  );
}
