// components/ProtectedRoute.tsx
"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";

export default function ValidationRole({
  children,
  role,
}: {
  role: string;
  children: React.ReactNode;
}) {
  const router = useRouter();
  const { user } = useAuth();

  useEffect(() => {
    if (!user) return; // Chưa login → chờ

    if (user.role != role) {
      router.replace("/403");
    }
  }, [user, router, role]);

  return <>{children}</>;
}
