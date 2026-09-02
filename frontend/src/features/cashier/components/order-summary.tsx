"use client";

import { Minus, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { formatOrderMoney as formatPriceVN } from "@/utils/format";
import { ServiceItem } from "../types";
import { useTranslations } from "next-intl";
import type { PreviewOrderResponse } from "@/api/generated";
import { itemKey } from "../hooks/cashier-draft";

interface OrderSummaryProps {
  items: ServiceItem[];
  onRemoveItem: (itemId: string) => void;
  onUpdateQuantity: (itemId: string, quantity: number) => void;
  preview?: PreviewOrderResponse;
}

export function OrderSummary({
  items,
  onRemoveItem,
  onUpdateQuantity,
  preview,
}: OrderSummaryProps) {
  const t = useTranslations();
  if (items.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        {t("common.noData")}
      </div>
    );
  }

  return (
    <div className="space-y-4 mt-2">
      {items.map((item) => (
        <div
          key={itemKey(item)}
          className="flex justify-between items-center pb-4 border-b"
        >
          <div className="flex-1">
            <h4 className="font-medium">
              {item.name}{" "}
              <span className="text-secondary-foreground text-sm">
                - {item.unitRelationName}
              </span>
            </h4>
          </div>

          <div className="flex items-center gap-2">
            <div className="w-full text-right font-medium">
              {preview?.orderItems?.find(
                (line) =>
                  line.serviceId === item.id &&
                  line.unitRelationId === item.unitRelationId,
              )?.unitPrice !== undefined
                ? formatPriceVN(
                    preview.orderItems.find(
                      (line) =>
                        line.serviceId === item.id &&
                        line.unitRelationId === item.unitRelationId,
                    )!.unitPrice!,
                  )
                : "—"}
            </div>

            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() =>
                onUpdateQuantity(itemKey(item), Math.max(1, item.quantity - 1))
              }
            >
              <Minus className="h-4 w-4" />
            </Button>

            <span className="w-8 text-center">{item.quantity}</span>

            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => onUpdateQuantity(itemKey(item), item.quantity + 1)}
            >
              <Plus className="h-4 w-4" />
            </Button>

            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive"
              onClick={() => onRemoveItem(itemKey(item))}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>

          <div className="w-20 text-right font-medium">
            {preview?.orderItems?.find(
              (line) =>
                line.serviceId === item.id &&
                line.unitRelationId === item.unitRelationId,
            )?.lineAmount !== undefined
              ? formatPriceVN(
                  preview.orderItems.find(
                    (line) =>
                      line.serviceId === item.id &&
                      line.unitRelationId === item.unitRelationId,
                  )!.lineAmount!,
                )
              : "—"}
          </div>
        </div>
      ))}
    </div>
  );
}
