"use client";
import { ShieldAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useRouter } from "next/navigation";
import { useTranslations } from "next-intl";
import { useMenuList } from "@/lib/menu-list";

export default function Page() {
  const t = useTranslations("errors.forbidden"); // Use translations for errors.forbidden namespace
  const router = useRouter();
  const menus = useMenuList(); // Get filtered menu list based on user role

  // Find the first valid route
  const firstValidRoute =
    menus.length > 0
      ? menus[0]?.submenus?.length && menus[0]?.submenus?.length > 0
        ? menus[0]?.submenus?.[0]?.href // Use first submenu's href if available
        : menus[0]?.href // Use top-level menu's href
      : "/"; // Fallback to root if no valid routes

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-background px-4">
      <div className="max-w-md w-full p-8 rounded-lg shadow-lg text-center">
        <div className="inline-flex h-20 w-20 items-center justify-center rounded-full bg-primary-foreground mb-6">
          <ShieldAlert className="h-10 w-10 text-primary" />
        </div>
        <h1 className="text-4xl font-bold text-primary mb-2">{t("title")}</h1>
        <p className="text-lg text-primary mb-2">{t("accessDenied")}</p>
        <p className="text-border mb-8">{t("noPermission")}</p>
        <Button className="bg-primary" asChild>
          <Button onClick={() => router.push(firstValidRoute)} variant={"link"}>
            {t("back")}
          </Button>
        </Button>
      </div>
    </div>
  );
}
