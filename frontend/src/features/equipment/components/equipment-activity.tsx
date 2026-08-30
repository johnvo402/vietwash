import { useEffect, useState } from "react";

import { DataTable } from "@/components/ui/table/data-table";
import { DateRange } from "react-day-picker";
import { EquipmentActivityFilters } from "./equipment-activity-filter";
import {
  useEquipmentActivity,
  useEquipmentMutations,
} from "../hooks/use-equipment";
import {
  GetEquipmentActivityDetailResponse,
  ListEquipmentActivityResponse,
  TypeActivity,
} from "@/api/generated/api";
import { useEquipmentActivityTable } from "./column";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";
import { useTranslations } from "next-intl";
import {
  EquipmentActivityFormData,
  EquipmentActivityFormDialog,
} from "./equiment-activity-create";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";

export default function EquipmentActivityPage({
  id,
  canCreate = false,
}: {
  id: number;
  canCreate: boolean;
}) {
  // Trạng thái cho bộ lọc
  const [time, setTime] = useState<DateRange | undefined>(undefined);
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const t = useTranslations();
  const { createEquipmentActivity, updateEquipmentActivity } =
    useEquipmentMutations();
  const [openCreate, setOpenCreate] = useState<boolean>(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<"detail" | "edit" | null>(null);
  const [selectedRow, setSelectedRow] =
    useState<ListEquipmentActivityResponse | null>(null);

  // Define the onAction callback
  const handleAction = (
    action: "detail" | "edit",
    row: ListEquipmentActivityResponse
  ) => {
    setSelectedRow(row);
    setDialogMode(action);
    // Do not set dialogOpen here; it will be set after data is fetched
  };

  // Query to fetch detailed data for the selected row
  const { data, isFetching } = useQuery<
    GetEquipmentActivityDetailResponse | undefined
  >({
    queryKey: ["equipment-detail", selectedRow?.id],
    queryFn: async () => {
      if (!selectedRow?.id) return undefined;
      const response =
        await apiClient.ecommerceApiEquipmentActivitiesDetailIdGet(
          selectedRow.id
        );
      return response.data.results;
    },
    enabled: !!selectedRow?.id, // Only run query if selectedRow.id exists
  });

  // Open dialog when data is fetched for "detail" or "edit" modes
  useEffect(() => {
    if (data && dialogMode && !isFetching) {
      setDialogOpen(true);
    }
  }, [data, dialogMode, isFetching]);
  const { columns } = useEquipmentActivityTable(handleAction);
  const {
    equipmentActivities,
    isLoading,
    error,
    paging,
    refetch: refetchQuery,
  } = useEquipmentActivity({
    time: time,
    type:
      typeFilter === "all"
        ? [TypeActivity.Repair, TypeActivity.Maintenance]
        : [typeFilter as TypeActivity],
    equipmentId: id,
  });
  // Hàm refetch để gọi lại dữ liệu (ví dụ: API call)
  const refetch = () => {
    refetchQuery();
  };

  const handleCreateActivity = async (data: EquipmentActivityFormData) => {
    if (dialogMode === "edit") {
      await updateEquipmentActivity({
        id: selectedRow?.id!,
        command: data,
      }).then(() => {
        refetch();
        setOpenCreate(false);
        setDialogOpen(false);
        setDialogMode(null);
        setSelectedRow(null);
      });
    } else {
      await createEquipmentActivity({
        id: id,
        command: data,
      }).then(() => {
        refetch();
        setOpenCreate(false);
        setDialogOpen(false);
        setDialogMode(null);
        setSelectedRow(null);
      });
    }
  };

  return (
    <>
      {/* Component bộ lọc */}
      <div className="flex justify-between">
        <EquipmentActivityFilters
          time={time}
          typeFilter={typeFilter}
          onApply={(dataTime, dataType) => {
            setTime(dataTime);
            setTypeFilter(dataType);
          }}
          refetch={refetch}
        />
        {canCreate && (
          <Button
            onClick={() => setOpenCreate(true)}
            className={cn(buttonVariants(), "text-xs md:text-sm")}
          >
            <Plus className="h-4 w-4" /> {t("common.create")}
          </Button>
        )}
      </div>

      <div className="mt-2">
        <DataTable
          columns={columns}
          data={equipmentActivities}
          loading={isLoading}
          paging={paging}
          error={error}
        />
      </div>
      {(openCreate || (dialogOpen && dialogMode && data)) && (
        <EquipmentActivityFormDialog
          isOpen={openCreate || dialogOpen}
          onClose={() => {
            setOpenCreate(false);
            setDialogOpen(false);
            setDialogMode(null);
            setSelectedRow(null);
          }}
          onSubmit={handleCreateActivity}
          initialData={
            openCreate
              ? null
              : {
                  description: data?.description ?? "",
                  details: (data?.details as any) ?? [],
                  laborCost: data?.laborCost ?? 0,
                  type: data?.type ?? "Maintenance",
                }
          }
          viewOnly={dialogMode === "detail"}
        />
      )}
    </>
  );
}
