// app/not-found.tsx
"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import LoadingSpinner from "@/components/main/LoadingSpinner";
import { useMenuList } from "@/lib/menu-list";

export default function NotFoundPage() {
  const router = useRouter();
  const menus = useMenuList(); // Get filtered menu list based on user role

  const firstValidRoute =
    menus.length > 0
      ? menus[0]?.submenus?.length && menus[0]?.submenus?.length > 0
        ? menus[0]?.submenus?.[0]?.href // Use first submenu's href if available
        : menus[0]?.href // Use top-level menu's href
      : "/"; // Fallback to root if no valid routes
  useEffect(() => {
    const timer = setTimeout(() => {
      router.replace(firstValidRoute);
    }, 2000);

    return () => clearTimeout(timer);
  }, [firstValidRoute, router]);

  return <LoadingSpinner />;
}
