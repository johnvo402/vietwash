"use client";

import type React from "react";

import { useState, useEffect } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { cn } from "@/lib/utils";
import { useTranslations } from "next-intl";

interface DiscountInputProps {
  className?: string;
  totalAmount?: number;
  onChange?: (value: {
    amount: number;
    isPercentage: boolean;
    error: string | null;
  }) => void;
}

export default function DiscountInput({
  className,
  totalAmount = 0,
  onChange,
}: DiscountInputProps) {
  const [isPercentage, setIsPercentage] = useState(false);
  const [value, setValue] = useState("");
  const [error, setError] = useState<string | null>(null);
  const t = useTranslations();
  // Validate whenever totalAmount changes
  useEffect(() => {
    validateInput(value);
    if (totalAmount == 0) {
      setValue("0");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [totalAmount]);

  const validateInput = (inputValue: string) => {
    if (inputValue === "") {
      setError(null);
      return;
    }

    const numericValue = Number.parseFloat(inputValue);

    // Validate based on discount type
    if (isPercentage) {
      if (numericValue > 100) {
        setError(t("cashier.errorMaxDiscount"));
      } else if (numericValue < 0) {
        setError(t("cashier.errorMinDiscount"));
      } else {
        // Calculate the actual amount based on percentage
        const calculatedAmount = (numericValue / 100) * totalAmount;
        if (calculatedAmount > totalAmount && totalAmount > 0) {
          setError(t("cashier.errorMaxAmount"));
        } else {
          setError(null);
        }
      }
    } else {
      if (numericValue < 0) {
        setError(t("cashier.errorMinAmount"));
      } else if (numericValue > totalAmount && totalAmount > 0) {
        setError(t("cashier.errorMaxAmount"));
      } else {
        setError(null);
      }
    }
  };

  const handleValueChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = e.target.value;

    // Allow empty input or numeric input with decimal point
    if (inputValue === "" || /^\d*\.?\d*$/.test(inputValue)) {
      setValue(inputValue);
      validateInput(inputValue);

      // Call onChange callback if provided
      if (onChange) {
        const numericValue =
          inputValue === "" ? 0 : Number.parseFloat(inputValue);
        onChange({
          amount: numericValue,
          isPercentage,
          error,
        });
      }
    }
  };

  const handleTypeToggle = (checked: boolean) => {
    setIsPercentage(checked);

    // Revalidate when switching types
    validateInput(value);

    // Call onChange callback if provided
    if (onChange && value) {
      const numericValue = Number.parseFloat(value);
      onChange({
        amount: numericValue,
        isPercentage: checked,
        error,
      });
    }
  };

  // Call onChange whenever error changes
  useEffect(() => {
    if (onChange && value) {
      const numericValue = Number.parseFloat(value);
      onChange({
        amount: numericValue,
        isPercentage,
        error,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [error]);

  return (
    <div className={cn("space-y-4", className)}>
      <div className="flex flex-col space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="discount">{t("order.discount")}</Label>
          <div className="flex items-center space-x-2">
            <Label
              htmlFor="discount-type"
              className="text-sm text-muted-foreground"
            >
              {isPercentage ? "Theo %" : "Theo tiền"}
            </Label>
            <Switch
              id="discount-type"
              checked={isPercentage}
              onCheckedChange={handleTypeToggle}
            />
          </div>
        </div>
        <div className="relative">
          <Input
            id="discount"
            type="text"
            value={value}
            onChange={handleValueChange}
            placeholder={
              isPercentage ? t("cashier.enterDiscount", { entity: "%" }) : t("cashier.enterDiscount", { entity: t("table.accessorKey.amount").replace(/^./, (c) => c.toLowerCase()) })
            }
            className={cn(error && "border-destructive")}
          />
          <div className="absolute inset-y-0 right-3 flex items-center pointer-events-none">
            <span className="text-muted-foreground">
              {isPercentage ? "%" : "₫"}
            </span>
          </div>
        </div>
        {error && <p className="text-sm text-destructive">{error}</p>}
      </div>
    </div>
  );
}
