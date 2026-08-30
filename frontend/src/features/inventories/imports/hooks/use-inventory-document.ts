import { apiClient } from "@/api/client";
import { useQueryFilter } from "@/lib/filter";
import {
  useQuery,
  useQueryClient,
  useMutation,
  useInfiniteQuery,
} from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";
import {
  InventoryStatus,
  CreateInventoryDocumentCommand,
  InventoryDocumentUpdateStatus,
  InventoryType,
  ListBranchProductResponse,
  ListSupplierResponse,
  ListUnitResponse,
} from "@/api/generated";
import { toast } from "react-toastify";
import { DateRange } from "@/features/reports/types/filter.type";

export const useInventoryDocumentsQuery = ({
  type,
  branchId = undefined,
  status,
  dateRange,
}: {
  type: InventoryType;
  branchId?: number;
  status: InventoryStatus[];
  dateRange: DateRange;
}) => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");

  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["code"] : undefined,
    filter: flattenQueryObject({
      type: {
        $eq: type,
      },
      ...(branchId ? { branchId: { $eq: branchId } } : {}),
      status: {
        $in: status,
      },
      $and: [
        {
          transactionAt: {
            $gte: new Date(dateRange.from).toISOString(),
          },
        },
        {
          transactionAt: {
            $lte: new Date(dateRange.to).toISOString(),
          },
        },
      ],
    }),
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
  });

  const {
    data,
    isFetching,
    isLoading: isQueryLoading,
    error: queryError,
  } = useQuery({
    queryKey: [
      "inventory-documents",
      { page, pageSize, search, status, branchId, dateRange },
    ],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiInventoriesGet(...args);
      return {
        documents: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  return {
    documents: data?.documents || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};
export const useInventoryDocumentMutations = (type: InventoryType) => {
  const queryClient = useQueryClient();
  const t = useTranslations();

  const createInventoryDocument = useMutation({
    mutationFn: async ({
      command,
      isNoti = false,
    }: {
      command: CreateInventoryDocumentCommand;
      isNoti?: boolean;
    }) => {
      const response = await apiClient.ecommerceApiInventoriesPost(command);
      return response.data;
    },
    onSuccess: (_, variables) => {
      const { isNoti } = variables; // Destructure variables
      queryClient.invalidateQueries({ queryKey: ["inventory-documents"] });
      if (isNoti) {
        toast.info(
          t("toast.create.success", {
            entity: t("common.inventoryDocument"),
          })
        );
      }
    },
    onError: (error: any) => {
      toast.error(
        t("toast.create.failed", {
          entity: t("common.inventoryDocument"),
        })
      );
    },
  });

  const updateInventoryDocument = useMutation({
    mutationFn: async ({
      oldId,
      command,
    }: {
      oldId: number;
      command: CreateInventoryDocumentCommand;
    }) => {
      const response = await apiClient.ecommerceApiInventoriesPost(command);
      return response.data;
    },
    onSuccess: async (data, variables) => {
      const { oldId } = variables;
      await apiClient.ecommerceApiInventoriesIdDelete(oldId);

      toast.info(
        t("toast.update.success", {
          entity: t("common.inventoryDocument"),
        })
      );
      queryClient.invalidateQueries({ queryKey: ["inventory-documents"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.update.failed", {
          entity: t("common.inventoryDocument"),
        })
      );
    },
  });

  const submitInventoryDocument = useMutation({
    mutationFn: async ({
      id,
      command,
    }: {
      id: number;
      command: InventoryDocumentUpdateStatus;
    }) => {
      const response = await apiClient.ecommerceApiInventoriesUpdateStatusIdPut(
        id,
        command
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inventory-documents"] });
      toast.info(
        t("toast.update.success", {
          entity: t("common.inventoryDocument"),
        })
      );
    },
    onError: (error: any) => {
      toast.error(
        t("toast.update.failed", {
          entity: t("common.inventoryDocument"),
        })
      );
    },
  });

  return {
    createInventoryDocument: createInventoryDocument.mutateAsync,
    updateStatusInventoryDocument: submitInventoryDocument.mutateAsync,
    isLoading:
      createInventoryDocument.isPending ||
      submitInventoryDocument.isPending ||
      updateInventoryDocument.isPending,
    updateInventoryDocument: updateInventoryDocument.mutateAsync,
  };
};

interface UseFormProductsResult {
  products: ListBranchProductResponse[];
  isLoading: boolean;
  error: any;
  fetchNextPage: () => void;
  hasNextPage: boolean | undefined;
}

export const useFormProducts = (
  searchTerm: string = "",
  productIds: number[] = []
): UseFormProductsResult => {
  const { flattenQueryObject, prepareApiParams } = useQueryFilter();

  const params = {
    page: 1,
    pageSize: productIds.length > 0 ? productIds.length : 5,
    searchKeyword: searchTerm || undefined,
    searchTargets: searchTerm ? ["name", "sku"] : undefined,
    ...(productIds.length > 0
      ? {
          filter: flattenQueryObject({
            id: {
              $in: productIds,
            },
          }),
        }
      : {}),
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

  const { data, isLoading, error, fetchNextPage, hasNextPage } =
    useInfiniteQuery({
      queryKey: ["form-products", { search: searchTerm }],
      queryFn: async ({ pageParam = 1 }) => {
        const args = prepareApiParams(searchApiParamsKeys, {
          ...params,
          page: pageParam,
        });
        const response = await apiClient.ecommerceApiBranchProductsGet(...args);
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

  const products = data?.pages.flatMap((page) => page.results) || [];

  return {
    products,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
  };
};

interface UseFormSuppliersResult {
  suppliers: ListSupplierResponse[];
  isLoading: boolean;
  error: any;
  fetchNextPage: () => void;
  hasNextPage: boolean | undefined;
}

export const useFormSuppliers = (
  searchTerm: string = ""
): UseFormSuppliersResult => {
  const { prepareApiParams } = useQueryFilter();

  const params = {
    page: 1,
    pageSize: 10,
    searchKeyword: searchTerm || undefined,
    searchTargets: searchTerm ? ["name", "code"] : undefined,
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

  const { data, isLoading, error, fetchNextPage, hasNextPage } =
    useInfiniteQuery({
      queryKey: ["form-suppliers", { search: searchTerm }],
      queryFn: async ({ pageParam = 1 }) => {
        const args = prepareApiParams(
          searchApiParamsKeys,
          { ...params, page: pageParam },
          { page: 1, pageSize: 10 }
        );
        const response = await apiClient.ecommerceApiSuppliersGet(...args);
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

  const suppliers = data?.pages.flatMap((page) => page.results) || [];

  return {
    suppliers,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
  };
};
interface UseFormUnitsResult {
  units: ListUnitResponse[];
  isLoading: boolean;
  error: unknown;
  fetchNextPage: () => void;
  hasNextPage: boolean | undefined;
}

export const useFormUnits = (searchTerm: string = ""): UseFormUnitsResult => {
  const { data, isLoading, error, fetchNextPage, hasNextPage } =
    useInfiniteQuery({
      queryKey: ["form-units", { search: searchTerm }],
      queryFn: async ({ pageParam = 1 }) => {
        const response = await apiClient.ecommerceApiUnitsGet(
          pageParam,
          10,
          undefined,
          undefined,
          searchTerm || undefined,
          searchTerm ? ["name"] : undefined
        );
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

  const units = data?.pages.flatMap((page) => page.results) || [];

  return {
    units,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
  };
};
