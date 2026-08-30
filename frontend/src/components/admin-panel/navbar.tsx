import { UserNav } from "@/components/admin-panel/user-nav";
import { SheetMenu } from "@/components/admin-panel/sheet-menu";
import { XBreadcrumb } from "../core/XBreadcrumb";
import BranchGlobalSelect from "../core/branch-global-select";
import { usePageType } from "@/hooks/use-page-type";
import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { ROUTE_CASHIER_ORDERS } from "@/types/router-type";
import NotificationClient from "../../features/notification/components/notification-client";
import { useIsMobile } from "@/hooks/use-mobile";
export function Navbar() {
  const { isCashierPage } = usePageType();

  const isMobile = useIsMobile();

  return (
    <header className="sticky top-0 z-40 w-full bg-background/95 shadow backdrop-blur supports-[backdrop-filter]:bg-background/60 dark:shadow-secondary">
      <div className="mx-4 sm:mx-8 flex h-14 items-center">
        <div className="flex items-center space-x-4 lg:space-x-0">
          <SheetMenu />
          {!isMobile && !isCashierPage && <XBreadcrumb />}
        </div>
        <div className="flex flex-1 items-center justify-end">
          {isCashierPage ? (
            <Link
              href={ROUTE_CASHIER_ORDERS}
              className="flex justify-start items-center hover:opacity-85 transition-opacity duration-300"
            >
              <ShoppingCart className="w-6 h-6 mr-3 text-primary" />
            </Link>
          ) : (
            <>
              <NotificationClient />
            </>
          )}

          <BranchGlobalSelect />
          <UserNav />
        </div>
      </div>
    </header>
  );
}
