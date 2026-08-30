"use client";

import { Minus, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { formatPriceVN } from "@/utils/format";
import { ServiceItem } from "../types";
import { useTranslations } from "next-intl";

interface OrderSummaryProps {
  items: ServiceItem[];
  onRemoveItem: (itemId: number) => void;
  onUpdateQuantity: (itemId: number, quantity: number) => void;
  onUpdatePrice: (itemId: number, price: number) => void; // New prop for price updates
}

export function OrderSummary({
  items,
  onRemoveItem,
  onUpdateQuantity,
  onUpdatePrice,
}: OrderSummaryProps) {
  const t = useTranslations();
  if (items.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        {t("common.noData")}
      </div>
    );
  }

  const handlePriceEditSave = (itemId: number, price: number) => {
    if (price >= 0) {
      onUpdatePrice(itemId, price);
    }
  };

  return (
    <div className="space-y-4 mt-2">
      {items.map((item) => (
        <div
          key={item.id}
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
              {formatPriceVN(item.price)}
            </div>

            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() =>
                onUpdateQuantity(item.id, Math.max(1, item.quantity - 1))
              }
            >
              <Minus className="h-4 w-4" />
            </Button>

            <span className="w-8 text-center">{item.quantity}</span>

            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => onUpdateQuantity(item.id, item.quantity + 1)}
            >
              <Plus className="h-4 w-4" />
            </Button>

            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive"
              onClick={() => onRemoveItem(item.id)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>

          <div className="w-20 text-right font-medium">
            {formatPriceVN(item.price * item.quantity || 0)}
          </div>
        </div>
      ))}
    </div>
  );
}
