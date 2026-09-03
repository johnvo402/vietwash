"use client";

import { useRef, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { Loader2 } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { apiClient } from "@/api/client";
import { OrderStatus } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import type { OrderEquipment } from "@/features/cashier/types";
import CashierEquipmentPicker from "./equipment-picker";
import { useOrderTransition } from "../compositions/use-order-transition";
import {
  availableOrderEquipmentFilter,
  isAvailableOrderEquipment,
} from "../order-lifecycle";
import { invalidateOrderEquipment } from "../order-lifecycle-cache";
import { getPaymentErrorMessage } from "../payments/payos";

interface StartOrderDialogProps {
  open: boolean;
  orderId: number;
  orderCode?: string | null;
  branchId: number;
  onOpenChange: (open: boolean) => void;
  onStarted?: () => void;
}

export function StartOrderDialog(props: StartOrderDialogProps) {
  // Closing/changing order drops dialog-only selections; no cross-order equipment reuse.
  return props.open ? (
    <StartOrderSession key={`${props.orderId}:${props.branchId}`} {...props} />
  ) : null;
}

function StartOrderSession({
  orderId,
  orderCode,
  branchId,
  onOpenChange,
  onStarted,
}: StartOrderDialogProps) {
  const t = useTranslations();
  const client = useQueryClient();
  const mutation = useOrderTransition();
  const { flattenQueryObject } = useQueryFilter();
  const [selected, setSelected] = useState<OrderEquipment[]>([]);
  const [search, setSearch] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const submitting = useRef(false);

  const submit = async () => {
    if (
      submitting.current ||
      branchId <= 0 ||
      !orderId ||
      selected.length === 0
    )
      return;
    submitting.current = true;
    setBusy(true);
    setError("");
    try {
      await mutation.mutateAsync({
        id: String(orderId),
        status: OrderStatus.InProgress,
        orderEquipments: selected.map(({ equipmentId }) => ({ equipmentId })),
      });
    } catch (failure) {
      setError(
        getPaymentErrorMessage(failure, t("order.updateOrderStatusFailed")),
      );
      await invalidateOrderEquipment(client);
      // Check selected IDs independently of the visible search/page; retain still-valid selections.
      try {
        const response = await apiClient.ecommerceApiEquipmentsGet(
          1,
          selected.length,
          undefined,
          undefined,
          undefined,
          undefined,
          undefined,
          flattenQueryObject({
            ...availableOrderEquipmentFilter(branchId),
            id: { $in: selected.map((x) => x.equipmentId) },
          }),
        );
        const available = new Set(
          (response.data.results?.data ?? [])
            .filter((x) => isAvailableOrderEquipment(x, branchId))
            .map((x) => x.id),
        );
        setSelected((previous) =>
          previous.filter((x) => available.has(x.equipmentId)),
        );
      } catch {
        // Availability is unknown, so require a fresh selection instead of allowing a stale retry.
        setSelected([]);
      }
      return;
    } finally {
      submitting.current = false;
      setBusy(false);
    }
    setSelected([]);
    onOpenChange(false);
    onStarted?.();
  };

  return (
    <Dialog
      open
      onOpenChange={(open) => {
        if (!submitting.current) onOpenChange(open);
      }}
    >
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {t("order.startProcessing")} {orderCode ? `#${orderCode}` : ""}
          </DialogTitle>
          <DialogDescription>
            {t("order.startEquipmentRequired")}
          </DialogDescription>
        </DialogHeader>
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          disabled={busy}
          aria-label={t("equipment.equipmentList.searchPlaceholder")}
          placeholder={t("equipment.equipmentList.searchPlaceholder")}
        />
        {error && (
          <p role="alert" className="text-sm text-destructive">
            {error} {t("order.reviewEquipment")}
          </p>
        )}
        <div className="max-h-[40vh] overflow-auto p-2">
          <CashierEquipmentPicker
            branchId={branchId}
            selected={selected}
            searchTerm={search}
            disabled={busy}
            onToggle={(equipment) => {
              if (submitting.current) return;
              setSelected((previous) =>
                previous.some((x) => x.equipmentId === equipment.equipmentId)
                  ? previous.filter(
                      (x) => x.equipmentId !== equipment.equipmentId,
                    )
                  : [...previous, equipment],
              );
            }}
          />
        </div>
        <DialogFooter>
          <Button
            variant="outline"
            disabled={busy}
            onClick={() => onOpenChange(false)}
          >
            {t("common.cancel")}
          </Button>
          <Button
            disabled={busy || branchId <= 0 || selected.length === 0}
            onClick={submit}
          >
            {busy && (
              <Loader2
                className="mr-2 h-4 w-4 animate-spin"
                aria-hidden="true"
              />
            )}
            {t(busy ? "common.status.handling" : "common.status.confirm")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
