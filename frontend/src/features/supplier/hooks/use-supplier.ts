import { apiClient } from "@/api/client";
import { CreateSupplierCommand, SupplierModel } from "@/api/generated";
import { useQueryFilter } from "@/lib/filter";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";
import { toast } from "react-toastify";

export const useSupplier = () => {
  const [page] = useQueryState("page", { defaultValue: "1" });
  const [pageSize] = useQueryState("pageSize", { defaultValue: "10" });
  const [search] = useQueryState("search");
  const { prepareApiParams } = useQueryFilter();
  const params = {
    page: parseInt(page) || 1,
    pageSize: parseInt(pageSize) || 10,
    searchKeyword: search || undefined,
    searchTargets: search ? ["name", "code"] : undefined,
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
    queryKey: ["suppliers", { page, pageSize, search }],
    queryFn: async () => {
      const response = await apiClient.ecommerceApiSuppliersGet(...args);
      return {
        suppliers: response.data.results?.data || [],
        paging: response.data.results?.paging || {},
      };
    },
  });

  return {
    suppliers: data?.suppliers || [],
    paging: data?.paging || {},
    isLoading: isQueryLoading || isFetching,
    error: queryError,
  };
};

// Hook chỉ để thực hiện các mutation create/update/delete
export const useSupplierMutations = () => {
  const t = useTranslations();
  const queryClient = useQueryClient();

  const createSupplierMutation = useMutation({
    mutationFn: async (supplierData: CreateSupplierCommand) => {
      return apiClient.ecommerceApiSuppliersPost(supplierData);
    },
    onSuccess: () => {
      toast.info(t("toast.create.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["suppliers"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.create.failed", { entity: t("common.supplier") }));
    },
  });

  const updateSupplierMutation = useMutation({
    mutationFn: async ({
      id,
      supplierData,
    }: {
      id: number;
      supplierData: SupplierModel;
    }) => {
      return apiClient.ecommerceApiSuppliersIdPut(id, supplierData);
    },
    onSuccess: () => {
      toast.info(t("toast.update.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["suppliers"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.update.failed", { entity: t("common.branch") }));
    },
  });

  const deleteSupplierMutation = useMutation({
    mutationFn: async ({ id }: { id: number }) => {
      return apiClient.ecommerceApiSuppliersIdDelete(id);
    },
    onSuccess: () => {
      toast.info(t("toast.delete.success", { entity: t("common.branch") }));
      queryClient.invalidateQueries({ queryKey: ["suppliers"] });
    },
    onError: (error: any) => {
      toast.error(t("toast.delete.failed", { entity: t("common.branch") }));
    },
  });

  const isLoading =
    createSupplierMutation.isPending ||
    updateSupplierMutation.isPending ||
    deleteSupplierMutation.isPending;

  const error =
    createSupplierMutation.error ||
    updateSupplierMutation.error ||
    deleteSupplierMutation.error;

  return {
    createSupplier: createSupplierMutation.mutate,
    updateSupplier: updateSupplierMutation.mutate,
    deleteSupplier: deleteSupplierMutation.mutate,
    isLoading,
    error,
  };
};
