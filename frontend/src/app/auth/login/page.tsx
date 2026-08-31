"use client";

import { useAuth } from "@/hooks/use-auth";
import { useRouter } from "nextjs-toploader/app";
import { useEffect } from "react";
import { ROUTE_DASHBOARD } from "@/types/router-type";
import LoginView from "@/features/auth/components/LoginView";

export default function LoginPage() {
  const isAuthenticated = useAuth((state) => state.isAuthenticated);
  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated) {
      router.push(ROUTE_DASHBOARD);
    }
  }, [isAuthenticated, router]);

  return (
    <div className="min-h-screen flex items-center justify-center dark:bg-black/[0.6] bg-gray-100">
      <LoginView />
    </div>
  );
}
