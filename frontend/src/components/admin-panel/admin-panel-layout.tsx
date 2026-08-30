"use client";

import { apiClient } from "@/api/client";
import { Footer } from "@/components/admin-panel/footer";
import { Sidebar } from "@/components/admin-panel/sidebar";
import { useAuth } from "@/hooks/use-auth";
import { useSidebar } from "@/hooks/use-sidebar";
import { useStore } from "@/hooks/use-store";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { useEffect } from "react";
import { Navbar } from "./navbar";
import { useIsMobile } from "@/hooks/use-mobile";
import { XBreadcrumb } from "../core/XBreadcrumb";
import { usePageType } from "@/hooks/use-page-type";

export default function AdminPanelLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { updateUser } = useAuth();
  const isMobile = useIsMobile();
  const { isCashierPage } = usePageType();

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
  const sidebar = useStore(useSidebar, (x) => x);
  if (!sidebar) return null;
  const { getOpenState, settings } = sidebar;
  return (
    <>
      <Sidebar />
      <main
        className={cn(
          "min-h-[calc(100vh_-_56px)] bg-primary-foreground transition-[margin-left] ease-in-out duration-300",
          !settings.disabled && (!getOpenState() ? "lg:ml-[90px]" : "lg:ml-72")
        )}
      >
        <Navbar />
        {isMobile && !isCashierPage && (
          <div className="pl-8 pt-6 text-xs">
            <XBreadcrumb />
          </div>
        )}
        {children}
      </main>
      <footer
        className={cn(
          "transition-[margin-left] ease-in-out duration-300 h-[56px] fixed bottom-0",
          !settings.disabled && (!getOpenState() ? "lg:ml-[90px]" : "lg:ml-72")
        )}
      >
        <Footer />
      </footer>
    </>
  );
}
