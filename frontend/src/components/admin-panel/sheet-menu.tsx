import { MenuIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Menu } from "@/components/admin-panel/menu";
import {
  Sheet,
  SheetHeader,
  SheetContent,
  SheetTrigger,
  SheetTitle,
} from "@/components/ui/sheet";
import Image from "next/image";
import { useTheme } from "next-themes";
import { useTranslations } from "next-intl";

export function SheetMenu() {
  const { theme } = useTheme();
  const t = useTranslations();

  return (
    <Sheet>
      <SheetTrigger className="lg:hidden" asChild>
        <Button
          className="h-11 w-11"
          variant="outline"
          size="icon"
          aria-label={t("common.openMenu")}
        >
          <MenuIcon size={20} aria-hidden="true" />
        </Button>
      </SheetTrigger>
      <SheetContent className="sm:w-72 px-3 h-full flex flex-col" side="left">
        <SheetHeader>
          <SheetTitle className="sr-only">
            {t("common.navigationMenu")}
          </SheetTitle>
          <Button
            className="flex justify-center items-center pb-2 pt-1"
            variant="link"
            asChild
          >
            <div className="flex justify-center items-center gap-2">
              <Image
                src={theme === "dark" ? "/logo-dark.png" : "/logo.png"}
                alt="logo"
                width={200}
                height={100}
                priority
                className="dark:bg-primary text-primary"
              />
            </div>
          </Button>
        </SheetHeader>
        <Menu isOpen />
      </SheetContent>
    </Sheet>
  );
}
