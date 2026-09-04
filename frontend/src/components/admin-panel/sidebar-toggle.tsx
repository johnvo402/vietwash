import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ChevronLeftIcon } from "@radix-ui/react-icons";
import { useTranslations } from "next-intl";

interface SidebarToggleProps {
  isOpen: boolean | undefined;
  setIsOpen?: () => void;
}

export function SidebarToggle({ isOpen, setIsOpen }: SidebarToggleProps) {
  const t = useTranslations();

  return (
    <div className="invisible lg:visible absolute bottom-[15%] -right-[16px] z-20">
      <Button
        onClick={() => setIsOpen?.()}
        className="rounded-md h-11 w-11"
        variant="outline"
        size="icon"
        aria-label={
          isOpen ? t("common.collapseSidebar") : t("common.expandSidebar")
        }
        aria-expanded={isOpen}
      >
        <ChevronLeftIcon
          className={cn(
            "h-4 w-4 transition-transform ease-in-out duration-700",
            isOpen === false ? "rotate-180" : "rotate-0",
          )}
          aria-hidden="true"
        />
      </Button>
    </div>
  );
}
