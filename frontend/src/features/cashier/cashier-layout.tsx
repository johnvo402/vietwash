"use client";
import { apiClient } from "@/api/client";
import { Navbar } from "@/components/admin-panel/navbar";
import { useAuth } from "@/hooks/use-auth";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";

export default function CashierLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { updateUser } = useAuth();
  const { data: me } = useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      return await apiClient.authApiAccountsProfileGet();
    },
  });
  useEffect(() => {
    if (me) {
      updateUser(me.data.results);
    }
  }, [me, updateUser]);
  return (
    <>
      <main
        className={cn(
          "min-h-[calc(100vh_-_56px)] bg-zinc-50 dark:bg-zinc-900 transition-[margin-left] ease-in-out duration-300"
        )}
      >
        <Navbar />
        {children}
      </main>
    </>
  );
}
