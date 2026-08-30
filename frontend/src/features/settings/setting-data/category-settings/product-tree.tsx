// Updated main component with React Query
"use client";

import { useState, useEffect } from "react";
import { Package, Plus, RefreshCw, Wifi, WifiOff } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useQueryClient } from "@tanstack/react-query";

import { TreeStatsComponent } from "@/components/tree/tree-stats";
import { TreeFiltersComponent } from "@/components/tree/tree-filters";
import { TreeLegend } from "@/components/tree/tree-legend";
import { TreeFilters } from "@/types/tree";
import LoadingSpinner from "@/components/main/LoadingSpinner";
import { ErrorMessage } from "@/components/core/error-message";
import { categoryKeys } from "./hooks/queries/use-categories-query";
import { useCategoryForm } from "./hooks/use-category-form";
import { useListCategoryResponseQuery } from "./hooks/use-category-data-query";
import { CategoryTreeNodeComponent } from "./components/category-tree-node";
import { CategoryFormQuery } from "./components/category-form";
import { CreateCategoryCommand } from "@/api/generated";
import { useTranslations } from "next-intl";

export default function CategoryTree() {
  const [filters, setFilters] = useState<TreeFilters>({
    searchTerm: "",
    showDisabled: true,
  });
  const t = useTranslations();

  const queryClient = useQueryClient();

  const {
    treeData,
    parentOptions,
    isLoading,
    isFetching,
    isRefetching,
    error,
    isError,
    isCreating,
    isUpdating,
    isDeleting,
    createCategory,
    updateCategory,
    deleteCategory,
    mutations,
  } = useListCategoryResponseQuery();

  const {
    isOpen,
    mode,
    editingNode,
    defaultParentId,
    openCreateForm,
    openEditForm,
    openAddChildForm,
    closeForm,
    getInitialFormData,
  } = useCategoryForm();

  // Close form on successful mutation
  useEffect(() => {
    if (mutations.create.isSuccess || mutations.update.isSuccess) {
      closeForm();
      mutations.create.reset();
      mutations.update.reset();
    }
  }, [
    mutations.create.isSuccess,
    mutations.update.isSuccess,
    closeForm,
    mutations,
  ]);

  const handleFormSubmit = async (
    formData: CreateCategoryCommand
  ): Promise<void> => {
    if (mode === "edit" && editingNode) {
      await updateCategory(editingNode.id!.toString(), formData);
    } else {
      await createCategory(formData);
    }
  };

  const handleDeleteCategory = async (nodeId: string): Promise<void> => {
    if (confirm(t("category.confirmDelete"))) {
      await deleteCategory(nodeId);
    }
  };

  const handleRefresh = () => {
    queryClient.invalidateQueries({ queryKey: categoryKeys.lists() });
  };

  return (
    <div className="w-full mx-auto space-y-6">
      {/* Network Status */}
      {/* {!isOnline && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <div className="flex items-center gap-2">
            <WifiOff className="w-5 h-5 text-yellow-600" />
            <span className="text-yellow-800">
              Không có kết nối mạng. Dữ liệu có thể không được cập nhật.
            </span>
          </div>
        </div>
      )} */}

      {/* Error Message */}
      {/* {isError && error && (
        <ErrorMessage
          message={error.message || t}
          onRetry={handleRefresh}
        />
      )} */}

      <Card className="rounded-none h-full">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <TreeFiltersComponent
                  filters={filters}
                  onFiltersChange={setFilters}
                  searchPlaceholder={t("category.searchPlaceholder")}
                  disabledLabel={t("common.disableAble")}
                />
                {/* {isOnline && <Wifi className="w-4 h-4 text-green-500" />} */}
                {/* Cache status */}
                {isRefetching && (
                  <Badge variant="outline" className="text-xs">
                    {t("common.updating")}...
                  </Badge>
                )}
              </CardTitle>
            </div>

            <div className="flex items-center gap-2">
              <Button
                onClick={openCreateForm}
                disabled={isCreating}
                className="flex items-center gap-2"
              >
                {isCreating ? <></> : <Plus className="w-4 h-4" />}
                {t("dialog.create.title", { entity: t("common.category") })}
              </Button>
            </div>
          </div>
          <TreeLegend />
        </CardHeader>

        <CardContent className="space-y-4">
          {/* Loading overlay for mutations */}
          <div className="relative">
            {(isUpdating || isDeleting) && (
              <div className="absolute inset-0 /50 backdrop-blur-sm z-10 flex items-center justify-center rounded-lg">
                <div className=" p-4 rounded-lg shadow-lg flex items-center gap-3">
                  <LoadingSpinner />
                  <span className="text-sm text-gray-600">
                    {isUpdating && t("common.updating") + "..."}
                    {isDeleting && t("common.deleting") + "..."}
                  </span>
                </div>
              </div>
            )}

            <div className="border rounded-lg p-4 max-h-full overflow-y-auto ">
              {treeData.size === 0 ? (
                <div className="text-center py-8 text-gray-500">
                  <Package className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                  <p>{t("common.noData")}</p>
                  <Button
                    onClick={openCreateForm}
                    className="mt-4"
                    disabled={isCreating}
                  >
                    {t("category.addFirst")}
                  </Button>
                </div>
              ) : (
                Array.from(treeData.values()).map((node) => (
                  <CategoryTreeNodeComponent
                    key={node.path}
                    node={node}
                    filters={filters}
                    onEdit={openEditForm}
                    onDelete={(node) =>
                      handleDeleteCategory(node.id!.toString())
                    }
                    onAddChild={openAddChildForm}
                  />
                ))
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Enhanced Form with React Query states */}
      {isOpen && (
        <CategoryFormQuery
          isOpen={isOpen}
          onClose={closeForm}
          onSubmit={handleFormSubmit}
          initialData={getInitialFormData()}
          mode={mode}
          parentOptions={parentOptions}
          defaultParentId={defaultParentId}
          isLoading={isCreating || isUpdating}
          error={mutations.create.error || mutations.update.error}
          isSuccess={mutations.create.isSuccess || mutations.update.isSuccess}
        />
      )}
    </div>
  );
}
