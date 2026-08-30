"use client";

import React, {
  useCallback,
  useEffect,
  useRef,
  useState,
  useMemo,
} from "react";
import { format } from "date-fns";
import {
  ArrowLeft,
  User,
  Clock,
  Package,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Loader2,
  Calendar,
  Phone,
  Printer,
  Banknote,
  GiftIcon,
  TableIcon,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogFooter,
} from "@/components/ui/alert-dialog";
import {
  CustomerGroup,
  GetOrderDetailResponse,
  OrderStatus,
  PaymentMethod,
} from "@/api/generated";
import { formatNumberVN, formatPriceVN } from "@/utils/format";
import { GetCustomerGroup } from "../../order-utils/order-util";
import PickupTicket, { PickupTicketRef } from "../PickupTicket";
import PrintBill, { PrintBillRef } from "../PrintBill";
import { useTranslations } from "next-intl";
import { Order } from "@/features/cashier/types";
import Image from "next/image";
import { PaymentModal } from "../payment-model";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

// Valid status transitions and configurations (unchanged)
const validTransitions: Partial<Record<OrderStatus, OrderStatus[]>> = {
  Pending: ["Pending", "InProgress"],
  InProgress: ["InProgress", "Processed"],
  Processed: ["Processed"],
  Completed: ["Completed"],
  Cancelled: ["Cancelled"],
};

const statusConfig = {
  Pending: {
    label: "pending",
    variant: "secondary" as const,
    icon: Clock,
    color: "bg-yellow-100 text-yellow-800 border-yellow-200",
  },
  InProgress: {
    label: "handling",
    variant: "default" as const,
    icon: Loader2,
    color: "bg-blue-100 text-blue-800 border-blue-200",
  },
  Processed: {
    label: "handled",
    variant: "secondary" as const,
    icon: Package,
    color: "bg-purple-100 text-purple-800 border-purple-200",
  },
  Completed: {
    label: "completed",
    variant: "default" as const,
    icon: CheckCircle2,
    color: "bg-green-100 text-green-800 border-green-200",
  },
  Cancelled: {
    label: "cancelled",
    variant: "error" as const,
    icon: XCircle,
    color: "bg-red-100 text-red-800 border-red-200",
  },
};

// Discount calculation (unchanged)
const getDiscountValue = (data: {
  amount: number;
  discountFixed: boolean;
  discountValue: number;
}): number => {
  if (data.discountFixed) {
    return data.discountValue;
  } else {
    return (data.discountValue / 100) * data.amount;
  }
};

interface OrderDetailProps {
  order?: GetOrderDetailResponse;
  onBack: () => void;
  onStatusChange: (orderId: string, newStatus: OrderStatus) => Promise<void>;
  onCancel: (orderId: string) => Promise<void>;
  getReceipt: () => Promise<void>;
  refetch: () => void;
}

export function OrderDetail({
  order,
  onBack,
  onStatusChange,
  onCancel,
  getReceipt,
  refetch,
}: OrderDetailProps) {
  const [status, setStatus] = useState<OrderStatus | undefined>(order?.status);
  const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isPaymentOpen, setIsPaymentOpen] = useState(false);
  const [isLoadingReceipt, setLoadingReceipt] = useState(false);

  const pickupTicketRef = useRef<PickupTicketRef>(null);
  const printBillRef = useRef<PrintBillRef>(null);
  const t = useTranslations();

  // Sync status with order prop changes
  useEffect(() => {
    setStatus(order?.status);
  }, [order?.status]);

  // Memoize payment success handler
  const handlePaymentSuccess = useCallback(
    (method: "cash" | "card") => {
      alert(
        t(method === "cash" ? "order.cashConfirmed" : "order.paymentSuccess")
      );
      refetch();
    },
    [t, refetch]
  );

  // Memoize receipt handler
  const handleGetReceipt = useCallback(async () => {
    setLoadingReceipt(true);
    try {
      await getReceipt();
    } catch (error) {
      console.error("Error getting receipt:", error);
      alert(t("order.errorGettingReceipt"));
    } finally {
      setLoadingReceipt(false);
    }
  }, [getReceipt, t]);

  // Memoize status change handler
  const handleStatusChange = useCallback(
    async (newStatus: OrderStatus) => {
      if (!order?.id) return;
      setIsUpdating(true);
      try {
        await onStatusChange(order.id.toString(), newStatus);
        setStatus(newStatus);
      } catch (error) {
        console.error("Error updating status:", error);
        alert(t("order.errorUpdatingStatus"));
      } finally {
        setIsUpdating(false);
      }
    },
    [order?.id, onStatusChange, t]
  );

  // Memoize cancel handler
  const handleCancel = useCallback(() => {
    setIsCancelDialogOpen(true);
  }, []);

  const confirmCancelOrder = useCallback(async () => {
    if (!order?.id) return;
    setIsUpdating(true);
    try {
      await onCancel(order.id.toString());
      setStatus("Cancelled");
    } catch (error) {
      console.error("Error cancelling order:", error);
      alert(t("order.errorCancellingOrder"));
    } finally {
      setIsUpdating(false);
      setIsCancelDialogOpen(false);
    }
  }, [order?.id, onCancel, t]);

  // Memoize print ticket handler
  const handlePrintTicket = useCallback(async () => {
    if (pickupTicketRef.current) {
      try {
        pickupTicketRef.current.print();
      } catch (error) {
        console.error("Error generating ticket:", error);
        alert(t("order.errorGeneratingTicket"));
      }
    }
  }, [t]);

  // Memoize print bill handler
  const handlePrintBill = useCallback(async () => {
    if (printBillRef.current) {
      try {
        printBillRef.current.print();
      } catch (error) {
        console.error("Error generating bill:", error);
        alert(t("order.errorGeneratingTicket"));
      }
    }
  }, [t]);

  // Memoize payment method formatter
  const getPaymentMethod = useCallback(
    (method: PaymentMethod) => {
      const methods: Record<PaymentMethod, string> = {
        [PaymentMethod.Cash]: t("fund.paymentMethod.cash"),
        [PaymentMethod.Card]: t("fund.paymentMethod.card"),
      };
      return methods[method] || t("common.status.unknown");
    },
    [t]
  );

  // Memoize order items for table
  const orderItems = useMemo(
    () => order?.orderItems || [],
    [order?.orderItems]
  );

  // Early return if no order
  if (!order) {
    return (
      <div className="flex flex-col items-center justify-center w-full h-full py-16 space-y-4">
        <AlertCircle className="w-16 h-16 text-muted-foreground" />
        <h2 className="text-2xl font-bold text-muted-foreground">
          {t("common.noData")}
        </h2>
        <Button onClick={onBack} variant="outline">
          <ArrowLeft className="w-4 h-4 mr-2" />
          {t("order.returnOrderList")}
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-2 w-full mx-auto p-4 md:p-6">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6 border border-blue-100">
        <div className="flex flex-col lg:flex-row lg:justify-between lg:items-start gap-2">
          <div className="flex items-start gap-4">
            <Button
              variant="outline"
              size="icon"
              onClick={onBack}
              className="shrink-0"
              disabled={isUpdating}
            >
              <ArrowLeft className="h-4 w-4" />
            </Button>
            <div className="space-y-2">
              <div className="flex items-center gap-3">
                <h1 className="text-2xl md:text-3xl font-bold text-gray-900">
                  #{order.code}
                </h1>
              </div>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row gap-3">
            {order.status === OrderStatus.Processed && (
              <Button
                onClick={() => setIsPaymentOpen(true)}
                disabled={isPaymentOpen || isUpdating}
                className="w-full sm:w-auto"
              >
                <Banknote className="w-4 h-4 mr-2" />
                {t("order.addPayment")}
              </Button>
            )}
            {order.status !== OrderStatus.Completed &&
              order.status !== OrderStatus.Cancelled && (
                <Button
                  onClick={handlePrintTicket}
                  disabled={isUpdating}
                  className="w-full sm:w-auto"
                >
                  <Printer className="w-4 h-4 mr-2" />
                  <PickupTicket ref={pickupTicketRef} order={order as Order} />
                  {t("order.printAppointmentSlip")}
                </Button>
              )}
            {order.status === OrderStatus.Completed && (
              <>
                <Button
                  onClick={handlePrintBill}
                  disabled={isUpdating}
                  className="w-full sm:w-auto"
                >
                  <Printer className="w-4 h-4 mr-2" />
                  <PrintBill ref={printBillRef} order={order as Order} />
                  {t("order.printBill")}
                </Button>
                <Button
                  onClick={handleGetReceipt}
                  disabled={isLoadingReceipt || isUpdating}
                  className="w-full sm:w-auto"
                >
                  {isLoadingReceipt ? (
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  ) : (
                    <Printer className="w-4 h-4 mr-2" />
                  )}
                  {t("order.deliveryReceipt")}
                </Button>
              </>
            )}
            <Select
              value={status}
              onValueChange={handleStatusChange}
              disabled={isUpdating}
            >
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder={t("common.status.selectStatus")} />
              </SelectTrigger>
              <SelectContent>
                {(validTransitions[status as OrderStatus] || []).map(
                  (nextStatus) => (
                    <SelectItem key={nextStatus} value={nextStatus}>
                      <div className="flex items-center gap-2">
                        {React.createElement(
                          statusConfig[nextStatus]?.icon || Clock,
                          { className: "w-4 h-4" }
                        )}
                        {t(
                          "common.status." +
                            (statusConfig[nextStatus]?.label || nextStatus)
                        )}
                      </div>
                    </SelectItem>
                  )
                )}
              </SelectContent>
            </Select>
            <Button
              variant="destructive"
              onClick={handleCancel}
              disabled={status === "Cancelled" || isUpdating}
              className="w-full sm:w-auto"
            >
              {isUpdating ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  {t("common.status.handling")}...
                </>
              ) : (
                <>
                  <XCircle className="w-4 h-4 mr-2" />
                  {t("common.cancel")}
                </>
              )}
            </Button>
          </div>
        </div>
      </div>

      {/* Content Grid */}
      <div className="grid lg:grid-cols-3 gap-2">
        <div className="lg:col-span-1">
          <Card className="shadow-sm">
            <CardHeader className="pb-3">
              <CardTitle className="flex items-center gap-2">
                <User className="w-5 h-5 text-blue-600" />
                {t("user.userInformation")}
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Customer Section */}
              <div>
                <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase tracking-wide">
                  {t("common.customer")}
                </h3>
                <div className="p-4 bg-gray-50 rounded-lg">
                  <div className="font-medium flex justify-between text-gray-900 mb-1">
                    {order.customer?.displayName ?? "--"}
                    {GetCustomerGroup(
                      t,
                      order.customer?.customerGroup ?? CustomerGroup.Normal
                    )}
                  </div>
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Phone className="w-4 h-4" />
                    {order.customer?.phoneNumber ?? "--"}
                  </div>
                </div>
              </div>
              <div>
                <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase tracking-wide">
                  {t("common.staff")}
                </h3>
                <div className="p-4 bg-gray-50 rounded-lg">
                  <div className="font-medium text-gray-900 mb-1">
                    {order.staff?.displayName ?? "--"}
                  </div>
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Phone className="w-4 h-4" />
                    {order.staff?.phoneNumber ?? "--"}
                  </div>
                </div>
              </div>
              <Separator />
              <div>
                <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase tracking-wide">
                  {t("order.time")}
                </h3>
                <div className="space-y-3">
                  <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
                    <Calendar className="w-4 h-4 text-blue-600" />
                    <div>
                      <div className="text-sm font-medium">
                        {t("table.accessorKey.createdAt")}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {order.createdAt
                          ? format(
                              new Date(order.createdAt),
                              "dd/MM/yyyy HH:mm"
                            )
                          : "--"}
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
                    <Clock className="w-4 h-4 text-green-600" />
                    <div>
                      <div className="text-sm font-medium">
                        {t("order.receivedAt")}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {order.orderDate
                          ? format(
                              new Date(order.orderDate),
                              "dd/MM/yyyy HH:mm"
                            )
                          : "--"}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <Separator />
              <div>
                <h3 className="font-semibold text-sm text-muted-foreground mb-3 uppercase tracking-wide">
                  {t("common.other")}
                </h3>
                <div className="space-y-3">
                  <div className="flex items-center gap-3 p-3 bg-primary-foreground rounded-lg">
                    <GiftIcon className="w-4 h-4 text-primary" />
                    <div>
                      <div className="text-sm font-medium">
                        {t("voucher.title")}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {order.voucherCode ?? "--"}
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 p-3 bg-primary-foreground rounded-lg">
                    <TableIcon className="w-4 h-4 text-primary" />
                    <div>
                      <div className="text-sm font-medium">
                        {t("common.tariff")}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {order.tariff?.name ?? "--"}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <Card className="lg:col-span-2 shadow-sm">
          <CardHeader className="pb-3">
            <CardTitle className="flex items-center gap-2">
              <Package className="w-5 h-5 text-purple-600" />
              {t("order.orderDetails")}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Tabs defaultValue="items" className="w-full mt-6">
              <TabsList>
                <TabsTrigger value="items">{t("order.orderItems")}</TabsTrigger>
                <TabsTrigger value="equipments">
                  {t("order.equipmentItems")}
                </TabsTrigger>
              </TabsList>

              {/* Tab Order Items */}
              <TabsContent value="items">
                <div className="overflow-x-auto mt-4">
                  <Table>
                    <TableHeader>
                      <TableRow className="bg-gray-50">
                        <TableHead>{t("table.accessorKey.index")}</TableHead>
                        <TableHead>{t("common.image")}</TableHead>
                        <TableHead>{t("common.service")}</TableHead>
                        <TableHead className="text-center">
                          {t("table.accessorKey.quantity")}
                        </TableHead>
                        <TableHead className="text-center">
                          {t("common.unit")}
                        </TableHead>
                        <TableHead className="text-right">
                          {t("common.price")}
                        </TableHead>
                        <TableHead className="text-right">
                          {t("table.accessorKey.total")}
                        </TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {orderItems.length > 0 ? (
                        orderItems.map((item, index) => (
                          <TableRow key={item.id}>
                            <TableCell>{index + 1}</TableCell>
                            <TableCell>
                              <div className="relative w-[40px] h-[40px]">
                                <Image
                                  src={item.serviceImage || "/logo/favicon.svg"}
                                  alt={t("image.alt")}
                                  fill
                                  style={{ objectFit: "contain" }}
                                />
                              </div>
                            </TableCell>
                            <TableCell>{item.serviceName}</TableCell>
                            <TableCell className="text-center">
                              {item.quantity}
                            </TableCell>
                            <TableCell className="text-center">
                              {item.unitRelationName}
                            </TableCell>
                            <TableCell className="text-right">
                              {formatPriceVN(item.unitPrice ?? 0)}
                            </TableCell>
                            <TableCell className="text-right">
                              {formatPriceVN(
                                (item.unitPrice ?? 0) * (item.quantity ?? 1)
                              )}
                            </TableCell>
                          </TableRow>
                        ))
                      ) : (
                        <TableRow>
                          <TableCell
                            colSpan={7}
                            className="text-center text-muted-foreground"
                          >
                            {t("common.noData")}
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </div>
              </TabsContent>

              {/* Tab Order Equipments */}
              <TabsContent value="equipments">
                <div className="overflow-x-auto mt-4">
                  <Table>
                    <TableHeader>
                      <TableRow className="bg-gray-50">
                        <TableHead>{t("table.accessorKey.index")}</TableHead>
                        <TableHead>{t("common.image")}</TableHead>
                        <TableHead>{t("table.accessorKey.code")}</TableHead>
                        <TableHead>
                          {t("common.entityName", {
                            Entity: t("equipment.title"),
                          })}
                        </TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {order.orderEquipments &&
                      order.orderEquipments?.length > 0 ? (
                        order.orderEquipments.map((equip, index) => (
                          <TableRow key={equip.code}>
                            <TableCell>{index + 1}</TableCell>
                            <TableCell>
                              <div className="relative w-[40px] h-[40px]">
                                <Image
                                  src={equip.image || "/logo/favicon.svg"}
                                  alt={t("image.alt")}
                                  fill
                                  style={{ objectFit: "contain" }}
                                />
                              </div>
                            </TableCell>
                            <TableCell>{equip.code}</TableCell>
                            <TableCell>{equip.equipmentName}</TableCell>
                          </TableRow>
                        ))
                      ) : (
                        <TableRow>
                          <TableCell
                            colSpan={4}
                            className="text-center text-muted-foreground"
                          >
                            {t("common.noData")}
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </div>
              </TabsContent>
            </Tabs>
            <Separator />
            <div className="bg-primary-foreground rounded-lg p-6">
              <h3 className="font-semibold text-lg mb-4">
                {t("order.orderSummary")}:
              </h3>
              <div className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    {t("order.subTotal")}:
                  </span>
                  <span className="font-medium">
                    {formatPriceVN(order.amount ?? 0)}
                  </span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    {t("order.discount")}:
                  </span>
                  <span className="font-medium text-green-600">
                    -
                    {formatPriceVN(
                      getDiscountValue({
                        amount: order.amount ?? 0,
                        discountFixed: order.discountFixed ?? false,
                        discountValue: order.discountValue ?? 0,
                      })
                    )}
                  </span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    {t("cashier.pointsDeduction")}
                    <span className="font-medium text-destructive">
                      ({formatNumberVN(order.point ?? 0)})
                    </span>
                    :
                  </span>
                  <span className="font-medium text-destructive">
                    {formatPriceVN((order.point ?? 0) * 10)}
                  </span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    {t("fund.paymentMethod.title")}:
                  </span>
                  <div className="font-medium text-gray-900">
                    {order.paymentMethod
                      ? getPaymentMethod(order.paymentMethod)
                      : "--"}
                  </div>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">
                    VAT({order.vat}%):
                  </span>
                  <span className="font-medium text-green-600">
                    {formatPriceVN(order.vatAmount)}
                  </span>
                </div>
                <Separator />
                <div className="flex justify-between text-lg font-bold">
                  <span>{t("table.accessorKey.total")}:</span>
                  <span className="text-blue-600">
                    {formatPriceVN(order.total ?? 0)}
                  </span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <AlertDialog
        open={isCancelDialogOpen}
        onOpenChange={setIsCancelDialogOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle className="flex items-center gap-2">
              <AlertCircle className="w-5 h-5 text-red-500" />
              {t("common.deleteConfirm.title", { entity: t("order.title") })}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {t("common.deleteConfirm.description", {
                entity: t("order.title"),
                entityName: `<strong>#${order.code}</strong>`,
              })}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isUpdating}>
              {t("common.deleteConfirm.cancel")}
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmCancelOrder}
              disabled={isUpdating}
              className="bg-red-600 hover:bg-red-700"
            >
              {isUpdating ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  {t("common.status.cancelling")}...
                </>
              ) : (
                t("common.deleteConfirm.confirm")
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {isPaymentOpen && (
        <PaymentModal
          isOpen={isPaymentOpen}
          onClose={() => setIsPaymentOpen(false)}
          onPaymentSuccess={handlePaymentSuccess}
          stepDefault="paymentMethod"
          orderDefault={order}
        />
      )}
    </div>
  );
}
