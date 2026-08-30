"use client";
import { ColumnDef } from "@tanstack/react-table";
import { useTranslations } from "next-intl";
import { useStringUtil } from "@/lib/stringUtil";
import Image from "next/image";
import { format } from "date-fns";
import { createContext, useContext, useMemo, useState } from "react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useRouter } from "nextjs-toploader/app";
import { formatPriceVN } from "@/utils/format";
import { usePushRouter } from "@/utils/router-utli";
import { ListServiceResponse } from "@/api/generated/api";
import { ROUTE_SERVICE_DETAIL, ROUTE_SERVICE_EDIT } from "@/types/router-type";

// Context for managing selected units across components
interface UnitSelectionContextType {
  selectedUnits: Record<string, string>;
  setSelectedUnit: (serviceId: string, unitId: string) => void;
}

const UnitSelectionContext = createContext<
  UnitSelectionContextType | undefined
>(undefined);

const useUnitSelection = () => {
  const context = useContext(UnitSelectionContext);
  if (!context) {
    throw new Error(
      "useUnitSelection must be used within a UnitSelectionProvider"
    );
  }
  return context;
};

// Provider component to wrap the table
export const UnitSelectionProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [selectedUnits, setSelectedUnits] = useState<Record<string, string>>(
    {}
  );

  const setSelectedUnit = (serviceId: string, unitId: string) => {
    setSelectedUnits((prev) => ({ ...prev, [serviceId]: unitId }));
  };

  const value = useMemo(
    () => ({ selectedUnits, setSelectedUnit }),
    [selectedUnits]
  );

  return (
    <UnitSelectionContext.Provider value={value}>
      {children}
    </UnitSelectionContext.Provider>
  );
};

export const useServiceTable = () => {
  const t = useTranslations();
  const router = useRouter();
  const { processText } = useStringUtil();

  // Status mapping

  // Unit selector component
  const UnitSelector: React.FC<{ service: ListServiceResponse }> = ({
    service,
  }) => {
    const { selectedUnits, setSelectedUnit } = useUnitSelection();

    const availableUnits = useMemo(
      () =>
        service?.unitRelations?.map((relation) => ({
          id: relation.id,
          name: relation.name,
        })),
      [service.unitRelations]
    );

    const defaultUnitId = useMemo(() => {
      const baseUnit = service?.unitRelations?.find(
        (relation) => relation.baseUnit
      );
      return baseUnit
        ? baseUnit.id
        : (service.unitRelations ? service.unitRelations.length > 0 : false)
          ? service.unitRelations
            ? service.unitRelations[0].id
            : ""
          : "";
    }, [service.unitRelations]);

    const selectedUnitId = selectedUnits[service.id!] || defaultUnitId;

    const handleUnitChange = (unitId: string) => {
      setSelectedUnit(service.id!.toString(), unitId);
    };

    if (!availableUnits?.length) {
      return <span className="text-gray-500">{t("common.noData")}</span>;
    }

    return (
      <Select
        value={selectedUnitId?.toString()}
        onValueChange={handleUnitChange}
      >
        <SelectTrigger className="w-[80px]">
          <SelectValue>
            {availableUnits.find((unit) => unit.id === selectedUnitId)?.name ||
              "--"}
          </SelectValue>
        </SelectTrigger>
        <SelectContent>
          {availableUnits.map((unit) => (
            <SelectItem key={unit.id} value={unit.id?.toString()!}>
              {unit.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    );
  };

  const PriceDisplay: React.FC<{ service: ListServiceResponse }> = ({
    service,
  }) => {
    const { selectedUnits } = useUnitSelection();

    const selectedUnitId =
      selectedUnits[service.id!] ??
      service.unitRelations?.find((r) => r.baseUnit)?.id?.toString() ??
      (service.unitRelations && service.unitRelations.length > 0
        ? String(service.unitRelations[0]?.id ?? "")
        : "");

    const selectedUnitRelation = service.unitRelations
      ? service.unitRelations.find((r) => r.id === Number(selectedUnitId))
      : undefined;

    if (!selectedUnitRelation) {
      return <span className="text-gray-500">-</span>;
    }

    return <span>{formatPriceVN(selectedUnitRelation.price!)}</span>;
  };

  // Table columns definition
  const columns: ColumnDef<ListServiceResponse>[] = [
    {
      accessorKey: "index",
      header: t("table.accessorKey.index"),
      cell: ({ row, table }) => {
        const pageIndex = table.getState().pagination.pageIndex;
        const pageSize = table.getState().pagination.pageSize;
        return pageIndex * pageSize + row.index + 1;
      },
    },
    {
      accessorKey: "image",
      header: t("common.image"),
      cell: ({ row }) => {
        const image = row.original.image;
        return (
          <div className="flex items-center space-x-2">
            <div className="w-10 h-10 relative rounded-sm overflow-hidden">
              <Image
                src={image && image !== "string" ? image : "/logo/favicon.svg"}
                alt={t("image.alt", { entity: t("common.service") })}
                className="rounded-sm"
                fill
                style={{ objectFit: "contain" }}
              />
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "name",
      header: t("table.accessorKey.name"),
      cell: ({ getValue }) => {
        const value = getValue() as string;
        return <span>{processText(value)}</span>;
      },
    },
    {
      accessorKey: "unit",
      header: t("common.unit").replace(/^./, (c) => c.toUpperCase()),
      cell: ({ row }) => <UnitSelector service={row.original} />,
    },
    {
      accessorKey: "price",
      header: t("common.price"),
      cell: ({ row }) => <PriceDisplay service={row.original} />,
    },
    {
      accessorKey: "category.name",
      header: t("common.category").replace(/^./, (c) => c.toUpperCase()),
    },
    {
      accessorKey: "createdAt",
      header: t("table.accessorKey.createdAt"),
      cell: ({ getValue }) => {
        const value = getValue() as string;
        try {
          const date = new Date(value);
          if (isNaN(date.getTime())) throw new Error("Invalid date");
          return format(date, "dd/MM/yyyy HH:mm:ss");
        } catch {
          return (
            <span className="text-gray-500">
              {t("table.accessorKey.invalidDate")}
            </span>
          );
        }
      },
    },
    {
      accessorKey: "status",
      header: t("common.status.title"),
      cell: ({ getValue }) => {
        const value = getValue() as string;
        return (
          <span
            className={value === "Active" ? "text-green-500" : "text-red-500"}
          >
            {t(`common.status.${value.toLowerCase()}`)}
          </span>
        );
      },
    },
    {
      id: "actions",
      cell: ({ row }) => {
        const service = row.original;
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const pushRouter = usePushRouter();
        const handleEdit = () => {
          pushRouter.pushRouter({
            router: ROUTE_SERVICE_EDIT,
            params: {
              publicId: service.publicId?.toString()!,
            },
            state: {
              [service.publicId?.toString()!]: service.id,
            },
          });
        };
        const handleDetail = () => {
          pushRouter.pushRouter({
            router: ROUTE_SERVICE_DETAIL,
            params: {
              publicId: service.publicId?.toString()!,
            },
            state: {
              [service.publicId?.toString()!]: service.id,
            },
          });
        };

        return (
          <div className="text-right">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0">
                  <span className="sr-only">{t("table.menu")}</span>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent>
                <DropdownMenuItem onClick={handleDetail}>
                  {t("common.details")}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleEdit}>
                  {t("common.update")}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        );
      },
    },
  ];

  return { columns, UnitSelectionProvider };
};

// Example usage component
