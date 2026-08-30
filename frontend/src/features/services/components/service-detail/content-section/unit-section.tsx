import { useState, Fragment } from "react";
import { useTranslations } from "next-intl";
import { UnitRelationProjection } from "@/api/generated/api";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { ChevronsUpDown } from "lucide-react";
import { formatPriceVN } from "@/utils/format";

type ServiceResourceView = {
  unitName: string;
  productName: string;
  quantity: number;
};

interface UnitListDisplayProps {
  unitRelations?: UnitRelationProjection[];
}

export default function UnitListDisplay({
  unitRelations,
}: UnitListDisplayProps) {
  const t = useTranslations();
  const [openMap, setOpenMap] = useState<Record<string | number, boolean>>({});
  const timeUnit = t("service.processingTime.unit");

  return (
    <Card className="w-full">
      <CardHeader>
        <CardTitle>
          {t("service.dialog.unit.relation.title", { fallback: "Units" })}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[45%]">
                {t("common.entityName", {
                  Entity: t("common.unit").replace(/^./, (c) =>
                    c.toUpperCase()
                  ),
                })}
              </TableHead>
              <TableHead className="w-[25%]">{t("common.price")}</TableHead>
              <TableHead className="w-[20%]">
                {t("service.processingTime.title")}
              </TableHead>
              <TableHead className="w-[10%] text-right">
                {t("service.resources.title")}
              </TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {unitRelations?.map((relation, index) => {
              const rowKey = (relation as any)?.id ?? index;
              const isOpen = !!openMap[rowKey];
              const res = relation.serviceResources ?? [];

              return (
                <Fragment key={rowKey}>
                  {/* Row chính */}
                  <TableRow>
                    <TableCell className="font-medium">
                      <div className="flex items-center gap-2">
                        {relation.name}
                        {relation.baseUnit && (
                          <Badge className="bg-primary text-xs">
                            {t("service.dialog.baseUnit")}
                          </Badge>
                        )}
                      </div>
                    </TableCell>

                    <TableCell>{formatPriceVN(relation?.price ?? 0)}</TableCell>

                    <TableCell>
                      {relation.processingTime ?? 0} {timeUnit}
                    </TableCell>

                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="gap-2"
                        onClick={() =>
                          setOpenMap((m) => ({ ...m, [rowKey]: !m[rowKey] }))
                        }
                        aria-expanded={isOpen}
                        aria-controls={`res-row-${rowKey}`}
                      >
                        <ChevronsUpDown className="h-4 w-4" />
                        <span className="text-xs text-muted-foreground">
                          {res.length} {t("service.resources.title")}
                        </span>
                      </Button>
                    </TableCell>
                  </TableRow>

                  {/* Row chi tiết (expand) */}
                  <TableRow
                    id={`res-row-${rowKey}`}
                    className={isOpen ? "" : "hidden"}
                  >
                    <TableCell colSpan={4} className="p-0 border-t-0">
                      <div className="px-4 pb-4">
                        <div className="rounded-md border">
                          <Table>
                            <TableHeader>
                              <TableRow>
                                <TableHead className="w-[50%]">
                                  {t("common.product")}
                                </TableHead>
                                <TableHead className="w-[30%]">
                                  {t("common.unit")}
                                </TableHead>
                                <TableHead className="w-[20%]">
                                  {t("table.accessorKey.quantity")}
                                </TableHead>
                              </TableRow>
                            </TableHeader>
                            <TableBody>
                              {res.length === 0 ? (
                                <TableRow>
                                  <TableCell
                                    colSpan={3}
                                    className="text-muted-foreground"
                                  >
                                    {t("common.noData")}
                                  </TableCell>
                                </TableRow>
                              ) : (
                                res.map((r, i) => (
                                  <TableRow key={i}>
                                    <TableCell>{r.productName}</TableCell>
                                    <TableCell>{r.unitName}</TableCell>
                                    <TableCell>{r.quantity}</TableCell>
                                  </TableRow>
                                ))
                              )}
                            </TableBody>
                          </Table>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                </Fragment>
              );
            })}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
