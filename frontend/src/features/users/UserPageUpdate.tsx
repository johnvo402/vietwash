"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import { useUserMutations } from "./hooks/use-user-hook";
import {
  CreateUserDialog,
  FormValues,
} from "./components/user-create/create-user-dialog";
import {
  UpdateAccount,
  GetAccountDetailResponse,
  MediaType,
} from "@/api/generated";
import { useTranslations } from "next-intl";
import { useEffect, useState } from "react";
import { useSearchParams } from "next/navigation";
import { usePushRouter } from "@/utils/router-utli";
import { ROUTE_USERS } from "@/types/router-type";
import { toast } from "react-toastify";

export default function UserPageUpdate({ publicId }: { publicId: string }) {
  const pushRouter = usePushRouter();
  const { updateAccount } = useUserMutations();
  const t = useTranslations();
  const searchParams = useSearchParams();
  const params = Object.fromEntries(searchParams.entries());
  const [open, setOpen] = useState<boolean>(true);
  const [id, setId] = useState<number | null>(null);

  useEffect(() => {
    const storedId = sessionStorage.getItem(publicId);
    if (storedId) setId(Number(storedId));
  }, [publicId]);

  const {
    data: user,
    isLoading,
    error,
  } = useQuery<GetAccountDetailResponse | undefined>({
    queryKey: ["user", id],
    queryFn: async () => {
      if (!id)
        throw new Error(
          t("common.idRequired", {
            Entity: t("common.user").replace(/^./, (c) => c.toUpperCase()),
          })
        );
      const response = await apiClient.authApiAccountsDetailEndpoint(id);
      return response.data.results;
    },
    enabled: !!id,
  });

  // Handle user update
  async function handleUpdateUser(
    data: FormValues & { id: number },
    formData: FormData,
    avt?: File // Added to handle avatar file
  ) {
    try {
      let avtUrl: string | null | undefined = data.avatar
        ? undefined
        : user?.avtUrl;
      let removeAvatar: boolean = false;

      // Handle avatar upload
      if (avt) {
        const response = await apiClient.authApiMediaPost(
          [avt],
          MediaType.Image
        );
        console.log("Avatar upload response:", response);
        avtUrl = response.data.results?.key?.[0] || undefined;
        if (!avtUrl) {
          throw new Error("Failed to upload avatar");
        }
      } else if (!avt && !user?.avtUrl && formData.get("removeAvatar")) {
        removeAvatar = true; // Signal avatar removal
      }

      // Convert FormData to UpdateAccount
      const formDataEntries = Object.fromEntries(formData);
      const accountData: UpdateAccount = {
        displayName: data.displayName,
        email: data.email,
        phoneNumber: data.phoneNumber || undefined,
        gender: data.gender,
        birthDay: data.birthday?.toISOString() ?? undefined,
        status: data.status,
        role: data.role,
        branchAccounts: formDataEntries.branchAccounts
          ? JSON.parse(formDataEntries.branchAccounts as string)
          : undefined,
        avtUrl: avtUrl, // Include uploaded avatar URL or existing one
        ...(removeAvatar && { removeAvatar: true }), // Include removeAvatar flag if needed
      };

      // Call updateAccount
      updateAccount({ id: data.id, accountData });
    } catch (error: any) {
      console.error("Error updating user:", error);
      toast.info(t("toast.update.failed", { entity: t("common.user") }));
      throw error; // Let the dialog handle the error
    }
  }

  const onClose = () => {
    setOpen(false);
    pushRouter.pushRouter({
      router: ROUTE_USERS,
      query: params,
    });
  };

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  return (
    <CreateUserDialog
      open={open}
      onClose={onClose}
      onUpdateUser={handleUpdateUser}
      user={user}
    />
  );
}
