import { apiClient } from "@/api/client";
import { useQueryFilter } from "@/lib/filter";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";
import {
  CreateBranchProductCommand,
  BranchProductModel,
  ActivationStatus,
} from "@/api/generated";
import { toast } from "react-toastify";

export const useBranchProduct = ({
  statusFilter = undefined,
  categoryId = undefined,
  branchId = undefined,
}: {
  statusFilter?: ActivationStatus[];
  categoryId?: number | null;
  branchId?: number | null;
}) => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");

  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["name", "sku"] : undefined,
    filter: flattenQueryObject({
      ...(categoryId ? { categoryId: { $eq: categoryId } } : {}),
      ...(statusFilter ? { status: { $in: statusFilter } } : {}),
      ...(branchId ? { branchId: { $eq: branchId } } : {}),
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
      "branch-products",
      { page, pageSize, search, statusFilter, categoryId },
    ],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiBranchProductsGet(...args);
      return {
        products: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  return {
    products: data?.products || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};
export const useBranchProductMutations = () => {
  const t = useTranslations();
  const queryClient = useQueryClient();

  const createBranchProductMutation = useMutation({
    mutationFn: async (productData: CreateBranchProductCommand) => {
      return apiClient.ecommerceApiBranchProductsPost(productData);
    },
    onSuccess: () => {
      toast.info(
        t("toast.create.success", { entity: t("common.branchProduct") })
      );
      queryClient.invalidateQueries({ queryKey: ["branch-products"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.create.failed", { entity: t("common.branchProduct") })
      );
    },
  });

  const updateBranchProductMutation = useMutation({
    mutationFn: async ({
      id,
      productData,
    }: {
      id: number;
      productData: BranchProductModel;
    }) => {
      return apiClient.ecommerceApiBranchProductsIdPut(id, productData);
    },
    onSuccess: () => {
      toast.info(
        t("toast.update.success", { entity: t("common.branchProduct") })
      );
      queryClient.invalidateQueries({ queryKey: ["branch-products"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.update.failed", { entity: t("common.branchProduct") })
      );
    },
  });

  const deleteBranchProductMutation = useMutation({
    mutationFn: async ({ id }: { id: number }) => {
      return apiClient.ecommerceApiBranchProductsIdDelete(id);
    },
    onSuccess: () => {
      toast.info(
        t("toast.delete.success", { entity: t("common.branchProduct") })
      );
      queryClient.invalidateQueries({ queryKey: ["branch-products"] });
    },
    onError: (error: any) => {
      toast.error(
        t("toast.delete.failed", { entity: t("common.branchProduct") })
      );
    },
  });

  const isLoading =
    createBranchProductMutation.isPending ||
    updateBranchProductMutation.isPending ||
    deleteBranchProductMutation.isPending;

  const error =
    createBranchProductMutation.error ||
    updateBranchProductMutation.error ||
    deleteBranchProductMutation.error;

  return {
    createBranchProduct: createBranchProductMutation.mutate,
    updateBranchProduct: updateBranchProductMutation.mutate,
    deleteBranchProduct: deleteBranchProductMutation.mutate,
    isLoading,
    error,
  };
};

export const useBranchProductCard = (id: number) => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");

  const { prepareApiParams, flattenQueryObject } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["documentCode"] : undefined,
    filter: flattenQueryObject({
      productId: {
        $eq: id,
      },
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
    queryKey: ["branch-products-card", { page, pageSize, search, id }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiBranchProductCardInvGet(
        ...args
      );
      return {
        products: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
    enabled: !!id,
  });

  return {
    productCards: data?.products || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};
