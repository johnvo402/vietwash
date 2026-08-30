"use client";
import {
  CreateUserDialog,
  FormValues,
} from "./components/user-create/create-user-dialog";
import { useUserMutations } from "./hooks/use-user-hook";
import { ROUTE_USERS } from "@/types/router-type";
import { usePushRouter } from "@/utils/router-utli";
import { useSearchParams } from "next/navigation";
import { apiClient } from "@/api/client"; // Assuming this is your API client
import {
  CreateAccountCommand,
  Gender,
  AccountStatus,
  MediaType,
} from "@/api/generated";

export default function UserPageCreate() {
  const pushRouter = usePushRouter();
  const { createAccount } = useUserMutations();
  const searchParams = useSearchParams();
  const params = Object.fromEntries(searchParams.entries());

  async function handleCreateUser(data: { user: FormData; avt?: File }) {
    try {
      let avtUrl: string | null = null;
      if (data.avt) {
        const response = await apiClient.authApiMediaPost(
          [data.avt],
          MediaType.Image
        );
        console.log("Avatar upload response:", response);
        avtUrl = response.data.results?.key?.[0] || null;
        if (avtUrl) {
          data.user.append("avtUrl", avtUrl);
        }
      }
      const formData = Object.fromEntries(data.user);
      const createAccountCommand: CreateAccountCommand = {
        displayName: formData.displayName as string,
        email: formData.email as string,
        password: formData.password as string | undefined,
        phoneNumber: formData.phoneNumber as string | undefined,
        gender: formData.gender as Gender,
        birthDay: formData.birthday as string | undefined,
        status: formData.status as AccountStatus,
        role: formData.role as string,
        avtUrl: avtUrl || undefined,
        branchAccounts: formData.branchAccounts
          ? JSON.parse(formData.branchAccounts as string)
          : undefined,
      };
      await createAccount(createAccountCommand);
    } catch (error) {
      console.error("Error creating user:", error);
      throw error;
    }
  }

  const onClose = () => {
    pushRouter.pushRouter({
      router: ROUTE_USERS,
      query: params,
    });
  };

  return (
    <CreateUserDialog
      onClose={onClose}
      open={true}
      onCreateUser={handleCreateUser}
    />
  );
}
