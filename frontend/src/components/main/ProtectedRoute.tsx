// components/ProtectedRoute.tsx
"use client";

import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";
import { useMenuList } from "@/lib/menu-list";

export default function ProtectedRoute({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const menuList = useMenuList();
  const { user } = useAuth();

  useEffect(() => {
    if (!user) return; // Chưa login → chờ

    const allAllowedRoutes: string[] = [];

    for (const menu of menuList) {
      if (menu.href) allAllowedRoutes.push(menu.href);
      if (menu.submenus) {
        for (const submenu of menu.submenus) {
          allAllowedRoutes.push(submenu.href);
        }
      }
    }

    const matched = allAllowedRoutes.find((route) =>
      pathname.startsWith(route)
    );

    if (!matched) {
      router.replace("/403");
    }
  }, [pathname, user, menuList, router]);

  return <>{children}</>;
}
