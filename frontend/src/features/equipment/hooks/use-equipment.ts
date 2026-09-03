import { apiClient } from "@/api/client";
import {
  EquipmentActivityModel,
  EquipmentStatus,
  EquipmentUpdateModel,
  ListEquipmentActivityResponse,
  ListEquipmentActivityResponsePaging,
  ListEquipmentResponse,
  TypeActivity,
} from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { PropsQuery } from "@/types/props";
import {
  useInfiniteQuery,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";
import { DateRange } from "react-day-picker";
import { toast } from "react-toastify";

interface UseFormEquipmentsResult {
  equipments: ListEquipmentResponse[];
  isLoading: boolean;
  error: any;
  refetch: () => void;
  fetchNextPage: () => void;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
}

export const useFormEquipments = ({
  searchTerm = "",
  query = {},
  enabled = true,
}: {
  searchTerm: string;
  query?: PropsQuery;
  enabled?: boolean;
}): UseFormEquipmentsResult => {
  const { prepareApiParams, flattenQueryObject } = useQueryFilter();

  const params = {
    page: 1,
    pageSize: 9,
    searchKeyword: searchTerm || undefined,
    searchTargets: searchTerm ? ["name", "code"] : undefined,
    filter: flattenQueryObject(query.filter),
  };

  const searchApiParamsKeys = [
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;

  const {
    data,
    isLoading,
    error,
    refetch,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    queryKey: ["form-equipments", { search: searchTerm, query }],
    enabled,
    queryFn: async ({ pageParam = 1 }) => {
      const args = prepareApiParams(
        searchApiParamsKeys,
        { ...params, page: pageParam },
        { page: 1, pageSize: 9 },
      );
      const response = await apiClient.ecommerceApiEquipmentsGet(...args);
      return {
        results: response.data.results?.data || [],
        paging: response.data.results?.paging,
      };
    },
    getNextPageParam: (lastPage) => {
      const paging = lastPage.paging;
      const current_page = paging?.currentPage ?? 1;
      const total_pages = paging?.totalPage ?? 1;
      return current_page < total_pages ? current_page + 1 : undefined;
    },
    initialPageParam: 1,
  });

  const equipments = data?.pages.flatMap((page) => page.results) || [];

  return {
    equipments,
    isLoading,
    error,
    refetch,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  };
};

interface UseFormEquipmentsActivityResult {
  equipmentActivities: ListEquipmentActivityResponse[];
  paging: ListEquipmentActivityResponsePaging;
  isLoading: boolean;
  refetch: () => void;
  error: any;
}
interface UseFormEquipmentsActivityProps {
  time?: DateRange;
  type?: TypeActivity[];
  equipmentId: number;
}
export const useEquipmentActivity = (
  props: UseFormEquipmentsActivityProps,
): UseFormEquipmentsActivityResult => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });

  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
  };
  const searchApiParamsKeys = [
    "page",
    "pageSize",
    "before",
    "after",
    "searchKeyword",
    "searchTargets",
    "sort",
    "filter",
  ] as const;
  const args = prepareApiParams(searchApiParamsKeys, params, {
    page: 1,
    pageSize: 10,
    filter: flattenQueryObject({
      ...(props.type
        ? {
            type: {
              $in: props.type,
            },
          }
        : {}),
      ...(props.time
        ? {
            $and: [
              {
                createdAt: {
                  $gte: props.time.from,
                },
              },
              {
                createdAt: {
                  $lte: props.time.to,
                },
              },
            ],
          }
        : {}),
      equipmentId: {
        $eq: props.equipmentId,
      },
    }),
  });
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["equipment-activity", { props: props, page, pageSize }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiEquipmentActivitiesGet(
        ...args,
      );
      return {
        equipment_activitys: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  return {
    equipmentActivities: data?.equipment_activitys ?? [],
    isLoading,
    paging: data?.paging ?? {},
    error,
    refetch,
  };
};

export const useEquipmentMutations = () => {
  const queryClient = useQueryClient();
  const t = useTranslations();

  const updateEquipment = useMutation({
    mutationFn: async ({
      id,
      command,
    }: {
      id: number;
      command: EquipmentUpdateModel;
    }) => {
      const response = await apiClient.ecommerceApiEquipmentsIdPut(id, command);
      return response.data;
    },
    onSuccess: async () => {
      toast.info(
        t("toast.update.success", {
          entity: t("equipment.title").toLowerCase(),
        }),
      );
      queryClient.invalidateQueries({ queryKey: ["equipments"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.update.failed", {
          entity: t("equipment.title").toLowerCase(),
        }),
      );
    },
  });
  const createEquipmentActivity = useMutation({
    mutationFn: async ({
      id,
      command,
    }: {
      id: number;
      command: EquipmentActivityModel;
    }) => {
      const response = await apiClient.ecommerceApiEquipmentsIdActivitiesPost(
        id,
        command,
      );
      return response.data;
    },
    onSuccess: async () => {
      toast.info(
        t("toast.create.success", {
          entity: t("equipment.activity").toLowerCase(),
        }),
      );
      queryClient.invalidateQueries({ queryKey: ["equipments"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.create.failed", {
          entity: t("equipment.activity").toLowerCase(),
        }),
      );
    },
  });

  const updateEquipmentActivity = useMutation({
    mutationFn: async ({
      id,
      command,
    }: {
      id: number;
      command: EquipmentActivityModel;
    }) => {
      const response = await apiClient.ecommerceApiEquipmentActivitiesIdPut(
        id,
        command,
      );
      return response.data;
    },
    onSuccess: async () => {
      toast.info(
        t("toast.create.success", {
          entity: t("equipment.activity").toLowerCase(),
        }),
      );
      queryClient.invalidateQueries({ queryKey: ["equipments"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.create.failed", {
          entity: t("equipment.activity").toLowerCase(),
        }),
      );
    },
  });

  const updateStatus = useMutation({
    mutationFn: async ({
      id,
      status,
    }: {
      id: number;
      status: EquipmentStatus;
    }) => {
      const response = await apiClient.ecommerceApiEquipmentsUpdateStatusidPut(
        id,
        status,
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["equipment-documents"] });
      toast.info(
        t("toast.update.success", {
          entity: t("equipment.title").toLowerCase(),
        }),
      );
    },
    onError: (error: any) => {
      toast.error(
        t("toast.update.failed", {
          entity: t("equipment.title").toLowerCase(),
        }),
      );
    },
  });

  return {
    createEquipmentActivity: createEquipmentActivity.mutateAsync,
    updateEquipment: updateEquipment.mutateAsync,
    updateStatusEquipment: updateStatus.mutateAsync,
    updateEquipmentActivity: updateEquipmentActivity.mutateAsync,
    isLoading:
      updateEquipment.isPending ||
      updateStatus.isPending ||
      createEquipmentActivity.isPending ||
      updateEquipmentActivity.isPending,
  };
};
