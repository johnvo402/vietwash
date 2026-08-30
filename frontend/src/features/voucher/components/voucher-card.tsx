"use client";

import Image from "next/image";
import { format } from "date-fns";
import { CalendarDays, Tag } from "lucide-react";
import { useTranslations } from "next-intl";

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardFooter,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { ListVoucherResponse } from "@/api/generated";
import { formatPriceVN } from "@/utils/format";

interface VoucherCardProps {
  voucher: ListVoucherResponse;
  viewDetail: (id: number) => void;
  openEdit: (id: number) => void;
}

export function VoucherCard({
  voucher,
  viewDetail,
  openEdit,
}: VoucherCardProps) {
  const t = useTranslations(); // Initialize useTranslations hook

  const isFullyUsed =
    voucher.totalQuantity! > 0 &&
    voucher.usedQuantity! >= voucher.totalQuantity!;
  const currentStatus = isFullyUsed ? "Used" : voucher.status;

  // Format the discount value display
  const discountText = voucher.discountFixed
    ? `${t("voucher.form.fixedDiscount")} ${formatPriceVN(voucher.discountValue ?? 0)}`
    : `${t("voucher.form.percentDiscount")} ${voucher.discountValue}%`;

  // Calculate the progress percentage for the usage bar
  const progressValue =
    voucher.totalQuantity! > 0
      ? (voucher.usedQuantity! / voucher.totalQuantity!) * 100
      : 0;

  return (
    <Card
      className={`flex flex-col overflow-hidden ${
        currentStatus === "Used" ? "opacity-60 grayscale" : ""
      }`}
    >
      <div className="relative h-40 w-full">
        <Image
          src={voucher.imgUrl || "/logo/favicon.svg"}
          alt={voucher.title!}
          layout="fill"
          objectFit="cover"
          className="rounded-t-lg"
        />
        <Badge
          className={`absolute right-2 top-2 ${
            currentStatus === "Active"
              ? "bg-green-500 hover:bg-green-500/80"
              : "bg-red-500 hover:bg-red-500/80"
          }`}
        >
          {currentStatus === "Active"
            ? t("voucher.form.statusActive")
            : t("voucher.form.statusInactive")}
        </Badge>
      </div>
      <CardHeader>
        <CardTitle className="text-lg font-semibold">{voucher.title}</CardTitle>
      </CardHeader>
      <CardContent className="flex-grow space-y-3">
        <div className="flex items-center justify-between text-sm font-medium">
          <span className="text-primary">{discountText}</span>
          <div className="flex items-center gap-1 text-muted-foreground">
            <Tag className="h-4 w-4" />
            <span>{voucher.code}</span>
          </div>
        </div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <CalendarDays className="h-4 w-4" />
          <span>
            {format(new Date(voucher.startAt!), "dd/MM/yyyy HH:mm")} -{" "}
            {format(new Date(voucher.endAt!), "dd/MM/yyyy HH:mm")}
          </span>
        </div>
        {voucher.totalQuantity! > 0 && (
          <div className="space-y-1">
            <div className="flex justify-between text-xs text-muted-foreground">
              <span>
                {t("common.status.used", { entity: "" })}:{" "}
                {voucher.usedQuantity}
              </span>
              <span>
                {t("table.accessorKey.total")}: {voucher.totalQuantity}
              </span>
            </div>
            <Progress value={progressValue} className="h-2" />
          </div>
        )}
      </CardContent>
      <CardFooter className="flex gap-2">
        <Button
          className="flex-1 bg-transparent"
          variant="outline"
          onClick={() => viewDetail(voucher.id!)}
        >
          {t("common.viewDetails")}
        </Button>
        <Button className="flex-1" onClick={() => openEdit(voucher.id!)}>
          {t("common.edit")}
        </Button>
      </CardFooter>
    </Card>
  );
}
