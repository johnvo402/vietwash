"use client";

import { useState } from "react";
import { DataTable } from "@/components/ui/table/data-table";
import { useTableFilters } from "@/compositions/tables/use-table-filters";
import { DataTableSearch } from "@/components/ui/table/data-table-search";
import { useTranslations } from "next-intl";
import {
  GetCustomerDetailResponse,
  ListCustomerResponse,
} from "@/api/generated";
import {
  useCustomerMutations,
  useCustomerQuery,
} from "./hooks/use-customer-hook";
import { useCustomerTable } from "./components/customer-table/columns";
import {
  CustomerFormData,
  CustomerFormDialog,
} from "./components/user-create/create-customer-dialog";
import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_CUSTOMER_DETAIL } from "@/types/router-type";

export default function CustomerListingPage() {
  const t = useTranslations();
  const { pushRouter } = usePushRouter();
  const { users, paging, isLoading, error } = useCustomerQuery();
  const { createCustomer, updateCustomer } = useCustomerMutations();
  const { searchQuery, setPage, setSearchQuery } = useTableFilters();
  const [openCreateCustomer, setOpenCreateCustomer] = useState<boolean>(false);
  const [customerSelected, setCustomerSelected] =
    useState<ListCustomerResponse | null>(null);

  const { columns } = useCustomerTable({
    onDetail: (data) =>
      pushRouter({
        router: ROUTE_CUSTOMER_DETAIL,
        params: {
          publicId: data.publicId?.toString()!,
        },
        state: {
          [data.publicId?.toString()!]: data.id,
        },
      }),
    onEdit: (customer) => {
      setCustomerSelected(customer);
      setOpenCreateCustomer(true);
    },
  });

  const { data: customerDetail } = useQuery<
    GetCustomerDetailResponse | undefined
  >({
    queryKey: ["customer", customerSelected?.id],
    queryFn: async () => {
      if (!customerSelected?.id) return undefined;
      const response = await apiClient.authApiCustomersDetailEndpoint(
        customerSelected.id
      );
      return response.data.results;
    },
    enabled: !!customerSelected?.id,
  });

  const createCustomerHandle = async ({
    status,
    displayName,
    gender,
    phoneNumber,
    accountContact,
  }: CustomerFormData) => {
    try {
      if (!customerSelected) {
        await createCustomer.mutateAsync({
          displayName,
          gender,
          phoneNumber,
          accountContact: accountContact || undefined,
        });
      } else {
        await updateCustomer({
          id: customerSelected.id!,
          accountData: {
            displayName,
            gender,
            phoneNumber,
            status,
            accountContact: accountContact || undefined,
          },
        });
      }
    } catch (error) {
      console.error(error);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-stretch justify-between">
        <DataTableSearch
          placeholder={t("search.searchBy", {
            entity: (
              t("customer.displayName") +
              " " +
              t("user.and") +
              " " +
              t("order.customerPhone")
            ).toLowerCase(),
          })}
          searchQuery={searchQuery}
          setSearchQuery={setSearchQuery}
          setPage={setPage}
        />

        <Button
          onClick={() => {
            setCustomerSelected(null);
            setOpenCreateCustomer(true);
          }}
          className={cn(buttonVariants(), "text-xs md:text-sm")}
        >
          <Plus className="h-4 w-4" /> {t("common.create")}
        </Button>
      </div>

      <div className="rounded-md border shadow-sm">
        <DataTable
          columns={columns}
          data={users}
          paging={paging}
          loading={isLoading}
          error={error}
        />
      </div>

      <CustomerFormDialog
        isOpen={openCreateCustomer}
        onClose={() => {
          setOpenCreateCustomer(false);
          setCustomerSelected(null);
        }}
        pageType="manage"
        onSubmit={createCustomerHandle}
        customer={customerDetail as any}
      />
    </div>
  );
}
