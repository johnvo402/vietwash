"use client";

import { useAuth } from "@/hooks/use-auth";
import { useRouter } from "nextjs-toploader/app";
import { useEffect } from "react";
import { apiClient } from "@/api/client";
import { getLandingRoute } from "@/lib/auth-routing";
import { toUserProfile } from "@/types/user";
import { useQuery } from "@tanstack/react-query";
import LoginView from "@/features/auth/components/LoginView";

export default function LoginPage() {
  const isAuthenticated = useAuth((state) => state.isAuthenticated);
  const updateUser = useAuth((state) => state.updateUser);
  const router = useRouter();
  const {
    data: profile,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["login-profile"],
    enabled: isAuthenticated,
    staleTime: 0,
    gcTime: 0,
    retry: false,
    queryFn: async () => {
      const response = await apiClient.authApiAccountsProfileGet();
      const user = toUserProfile(response.data.results);
      if (!user) throw new Error("Invalid account profile");
      return user;
    },
  });

  useEffect(() => {
    if (isAuthenticated && profile) {
      updateUser(profile);
      router.replace(getLandingRoute(profile.role));
    }
  }, [isAuthenticated, profile, updateUser, router]);

  return (
    <div className="min-h-screen flex items-center justify-center dark:bg-black/[0.6] bg-gray-100">
      {isAuthenticated ? (
        <div role="status" className="text-center space-y-3">
          <p>
            {isError ? "Unable to load your account." : "Loading your account…"}
          </p>
          {isError && (
            <button className="underline" onClick={() => refetch()}>
              Retry
            </button>
          )}
        </div>
      ) : (
        <LoginView />
      )}
    </div>
  );
}
