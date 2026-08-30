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
  Search,
} from "lucide-react";
import { formatNumberVN, formatPriceVN, parseNumberVN } from "@/utils/format";
import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { Customer } from "@/utils/customer-indexedDb";
import { PriceItem } from "@/utils/tariff-db";
import CustomDateTime from "./booking-receipt-date";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { toast } from "react-toastify";
import { QRScanner } from "@/components/qr-scanner";

interface OrderPaymentProps {
  total: number;
  amount: number;
  discountValue: number;
  discountFixed: boolean;
  voucherCode: string;
  disable: boolean;
  isProcessing: boolean;
  handleProcessOrder: (value: {
    discountValue: number;
    discountFixed: boolean;
    voucherCode?: string;
    bookingDate?: Date;
  }) => void;
  handlePrint?: () => void;
  printDisabled?: boolean;
  handleApplyVoucher: (
    voucherCode: string,
    tabId: string
  ) => Promise<{
    discountValue: number;
    discountFixed: boolean;
    message: string;
  }>;
  activeTab: string;
  voucherDisabled?: boolean;
  customers: Customer[];
  tariffs: PriceItem[];
  tariffId: number;
  onSetTariff: (data: number | null) => void;
  note?: string;
  onSetNote: (note: string | null) => void;
  customerInit: Customer | null;
  onSelect: (customer: Customer | null) => void;
  openCreate?: () => void;
  point: number;
  handleUpdatePoints: (
    points: number,
    tabId: string,
    maxPoints: number
  ) => void;
  deliveryTime: string | null;
  isEdit?: boolean;
}

export const OrderPaymentSection = ({
  total,
  amount,
  discountValue,
  discountFixed,
  voucherCode: initialVoucherCode,
  disable,
  isProcessing,
  handleProcessOrder,
  handlePrint,
  printDisabled = false,
  voucherDisabled = true,
  handleApplyVoucher,
  activeTab,
  customers,
  tariffs,
  tariffId,
  onSetTariff,
  note: initialNote,
  onSetNote,
  customerInit,
  onSelect,
  openCreate,
  point,
  handleUpdatePoints,
  deliveryTime,
  isEdit = false,
}: OrderPaymentProps) => {
  const [voucherCode, setVoucherCode] = useState<string>(initialVoucherCode);
  const [bookingDate, setBookingDate] = useState<Date | undefined>(
    deliveryTime ? new Date(deliveryTime) : undefined
  );
  const [voucherMessage, setVoucherMessage] = useState<string>("");
  const [customer, setCustomer] = useState<Customer | null>(
    customerInit || null
  );
  const [tariff, setTariff] = useState<PriceItem | null>(null);
  const [searchTerm, setSearchTerm] = useState<string>("");

  const [note, setNote] = useState<string>(initialNote || "");
  const [points, setPoints] = useState<number>(point);
  const [pointsError, setPointsError] = useState<string>("");
  const [showQRScanner, setShowQRScanner] = useState<boolean>(false);
  const t = useTranslations();

  const discountAmount = discountFixed
    ? discountValue
    : amount * (discountValue / 100);
  const pointsDeduction = points * 10;
  const { data: pointData, refetch } = useQuery({
    queryKey: ["customerPoint", customer?.id],
    queryFn: async () =>
      await apiClient.financeApiTransactionGetPointByCustomerIdIdGet(
        customer?.id!
      ),
    enabled: !!customer?.id,
  });

  useEffect(() => {
    setVoucherCode(initialVoucherCode);
    setCustomer(customerInit || null);
    setNote(initialNote || "");
    setPoints(point);
    setBookingDate(deliveryTime ? new Date(deliveryTime) : undefined);
    setPointsError("");
    const selectedTariff = tariffs.find((x) => x.id === tariffId);
    if (selectedTariff) setTariff(selectedTariff);
    if (voucherDisabled) {
      setVoucherMessage("");
      setVoucherCode("");
    }
    refetch();
  }, [
    initialVoucherCode,
    customerInit,
    initialNote,
    tariffId,
    tariffs,
    voucherDisabled,
    refetch,
    point,
    deliveryTime,
  ]);

  const handleDateChange = (date: Date | undefined) => {
    setBookingDate(date);
  };

  const handleApplyVoucherClick = async () => {
    if (!voucherCode) {
      setVoucherMessage(t("cashier.enterVoucherCode"));
      return;
    }
    const result = await handleApplyVoucher(voucherCode, activeTab);
    setVoucherMessage(result.message);
  };

  const handleSubmit = () => {
    if (!bookingDate) {
      toast.error(t("cashier.pleaseSelectPickupTime"));
      return;
    }
    handleProcessOrder({
      discountValue,
      discountFixed,
      bookingDate,
      voucherCode,
    });
  };

  const handleSelectCustomer = (data: Customer | null) => {
    const updatedCustomer = data ? { ...data, note } : null;
    setCustomer(updatedCustomer);
    onSelect(updatedCustomer);
    setPoints(0);
    setPointsError("");
    handleUpdatePoints(0, activeTab, pointData?.data.results?.point ?? 0);
  };

  const handleSelectTariff = (id: string | null) => {
    const selectedTariff = tariffs.find((x) => x.id.toString() === id);
    if (selectedTariff) {
      setTariff(selectedTariff);
      onSetTariff(Number(id));
    }
  };

  const handleNoteChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newNote = e.target.value;
    setNote(newNote);
    onSetNote(newNote);
  };

  const handlePointsChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const parsedPoints = parseNumberVN(e.target.value);
    if (isNaN(parsedPoints)) {
      setPointsError(t("cashier.pointsMustBeNonNegative"));
      return;
    }
    if (parsedPoints < 0) {
      setPointsError(t("cashier.pointsMustBeNonNegative"));
      return;
    }
    if (parsedPoints > (pointData?.data.results?.point ?? 0)) {
      setPointsError(t("cashier.pointsExceedAvailable"));
      return;
    }
    setPointsError("");
    setPoints(parsedPoints);
    handleUpdatePoints(
      parsedPoints,
      activeTab,
      pointData?.data.results?.point ?? 0
    );
  };

  const handleQRScanSuccess = (scannedCode: string) => {
    setVoucherCode(scannedCode);
    setVoucherMessage("");
    setShowQRScanner(false);
    handleApplyVoucherClick();
  };

  const handleQRStop = () => {
    setShowQRScanner(false);
  };

  const getTariffOptions = () => {
    return (
      tariffs?.map((tariff: PriceItem) => ({
        value: tariff.id.toString(),
        label: tariff.name || t("common.unknown"),
      })) || []
    );
  };

  return (
    <>
      <Card className="bg-background shadow-md rounded-lg">
        <CardHeader>
          <CardTitle className="text-xl font-bold text-gray-800">
            {t("cashier.orderInformation")}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label className="text-sm font-medium">
                {t("user.customerInformation")}
              </Label>
              <div className="flex items-center gap-2">
                <Select
                  value={customer?.id.toString() || undefined}
                  disabled={isEdit}
                  onValueChange={(value) => {
                    if (value === "create") {
                      openCreate?.();
                    } else {
                      const selectedCustomer = customers.find(
                        (c) => c.id === Number(value)
                      );
                      handleSelectCustomer(selectedCustomer || null);
                    }
                  }}
                >
                  <SelectTrigger id="customer-select" className="flex-1">
                    <SelectValue
                      placeholder={t("common.placeholderSelect", {
                        entity: t("common.customer"),
                      })}
                    />
                  </SelectTrigger>
                  <SelectContent onCloseAutoFocus={() => setSearchTerm("")}>
                    <input
                      type="text"
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      placeholder={t("cashier.customerPlaceholder")}
                      className="w-full p-2 border-b focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                    {customers
                      .filter(
                        (c) =>
                          c.displayName
                            ?.toLowerCase()
                            .includes(searchTerm.toLowerCase()) ||
                          c.phoneNumber
                            ?.toLowerCase()
                            .includes(searchTerm.toLowerCase())
                      )
                      .map((c) => (
                        <SelectItem key={c.id} value={c.id.toString()}>
                          {c.displayName} -{" "}
                          {c.phoneNumber || t("common.noPhone")}
                        </SelectItem>
                      ))}
                    <SelectItem
                      value="create"
                      className="text-primary font-medium flex items-center justify-center w-full gap-2"
                    >
                      <Plus className="h-6 w-6" />
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="tariff" className="text-sm font-medium">
                {t("common.tariff")}
              </Label>
              <Combobox
                options={getTariffOptions()}
                value={
                  tariff?.id.toString() || tariffs[0]?.id?.toString() || ""
                }
                onChange={(value) => handleSelectTariff(value)}
                placeholder={t("common.entitySelectPlaceholder", {
                  entity: t("common.tariff"),
                })}
                searchPlaceholder={t("search.searchBy", {
                  entity: t("common.tariff"),
                })}
                emptyMessage={t("common.noOptions")}
                disabled={!tariffs.length}
              />
            </div>
            {customer && (
              <div className="text-primary font-medium">
                {t("cashier.point")}
                {formatNumberVN(pointData?.data.results?.point ?? 0)}
              </div>
            )}
          </div>

          <div>
            <Label htmlFor="customer-note" className="text-sm font-medium">
              {t("common.note")}
            </Label>
            <Input
              id="customer-note"
              value={note}
              onChange={handleNoteChange}
              placeholder={t("dialog.placeholder", {
                entity: t("common.note").toLowerCase(),
              })}
              className="mt-1 w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="points-input" className="text-sm font-medium">
              {t("cashier.usePoints")}
            </Label>
            <Input
              id="points-input"
              type="text"
              value={formatNumberVN(points)}
              onChange={handlePointsChange}
              placeholder={t("cashier.enterPoints")}
              disabled={!customer}
              min="0"
              max={pointData?.data.results?.point ?? 0}
              className="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {pointsError && (
              <p className="text-sm text-red-600">{pointsError}</p>
            )}
          </div>

          <CustomDateTime
            onChange={handleDateChange}
            showSeconds
            date={bookingDate}
            placeholder={t("cashier.selectPickupTime")}
          />

          <div className="space-y-2">
            <div className="flex items-center gap-2">
              <Input
                type="text"
                disabled={voucherDisabled || isEdit}
                value={voucherCode}
                onChange={(e) => {
                  setVoucherCode(e.target.value);
                  setVoucherMessage("");
                }}
                placeholder={t("cashier.enterVoucherCode")}
                className="flex-1 p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              <Button
                variant="outline"
                onClick={() => setShowQRScanner(!showQRScanner)}
                disabled={voucherDisabled || isEdit}
                className="px-4 py-2"
              >
                <QrCode className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                onClick={handleApplyVoucherClick}
                disabled={voucherDisabled || isEdit}
                className="px-4 py-2"
              >
                {t("common.apply")}
              </Button>
            </div>
            {showQRScanner && (
              <QRScanner
                onScanSuccess={handleQRScanSuccess}
                onScanError={(error) => toast.error(error)}
                onStop={handleQRStop}
                className="mt-4"
                autoStart={true}
              />
            )}
            {voucherMessage && (
              <p
                className={`text-sm ${
                  discountValue > 0 ? "text-green-600" : "text-red-600"
                }`}
              >
                {voucherMessage}
              </p>
            )}
          </div>
        </CardContent>
        <CardFooter className="sticky bottom-0 z-10 p-6 bg-background border-t border-secondary">
          <div className="space-y-6 flex items-center flex-col w-full">
            <div className="space-y-2 w-full">
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-700">
                  {t("table.accessorKey.amount")}:
                </span>
                <span className="text-lg font-bold text-gray-900">
                  {formatPriceVN(amount)}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-700">
                  {t("order.discount")}:
                </span>
                <span className="text-lg font-bold text-gray-900">
                  {formatPriceVN(discountAmount)}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-700">
                  {t("cashier.pointsDeduction")}:
                </span>
                <span className="text-lg font-bold text-gray-900">
                  {formatPriceVN(pointsDeduction)}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-700">
                  {t("VAT 10%")}:
                </span>
                <span className="text-lg font-bold text-gray-900">
                  {formatPriceVN(
                    (amount - discountAmount - pointsDeduction) * 0.1
                  )}
                </span>
              </div>
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-700">
                  {t("table.accessorKey.total")}:
                </span>
                <span className="text-lg font-bold text-gray-900">
                  {formatPriceVN(total)}
                </span>
              </div>
            </div>

            <div className="w-full flex gap-2">
              <Button
                disabled={printDisabled || isEdit}
                onClick={handlePrint}
                className="flex-1 bg-primary-foreground hover:opacity-25"
              >
                <Printer className="h-4 w-4 mr-2 text-primary" />
              </Button>
              <Button
                variant="default"
                className="flex-1 bg-primary text-background hover:opacity-25"
                disabled={disable || isProcessing}
                onClick={handleSubmit}
              >
                {isProcessing ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    {t("common.status.handling")}...
                  </>
                ) : isEdit ? (
                  <>
                    <Save className="mr-2 h-4 w-4" />
                    {t("cashier.updateOrder")}
                  </>
                ) : (
                  <>
                    <ShoppingCart className="mr-2 h-4 w-4" />
                    {t("cashier.createOrder")}
                  </>
                )}
              </Button>
            </div>
          </div>
        </CardFooter>
      </Card>
    </>
  );
};
