"use client";
import { Menu } from "@/components/admin-panel/menu";
import { SidebarToggle } from "@/components/admin-panel/sidebar-toggle";
import { useSidebar } from "@/hooks/use-sidebar";
import { useStore } from "@/hooks/use-store";
import { cn } from "@/lib/utils";
import { useTheme } from "next-themes";
import Image from "next/image";

export function Sidebar() {
  const sidebar = useStore(useSidebar, (x) => x);
  const { theme } = useTheme();
  if (!sidebar) return null;
  const { isOpen, toggleOpen, getOpenState, setIsHover, settings } = sidebar;
  return (
    <aside
      className={cn(
        "fixed top-0 left-0 z-20 h-screen -translate-x-full lg:translate-x-0 transition-[width] ease-in-out duration-300",
        !getOpenState() ? "w-[72px]" : "w-72",
        settings.disabled && "hidden"
      )}
    >
      <SidebarToggle isOpen={isOpen} setIsOpen={toggleOpen} />
      <div
        onMouseEnter={() => setIsHover(true)}
        onMouseLeave={() => setIsHover(false)}
        className="relative h-full flex flex-col overflow-y-auto shadow-md pt-3 dark:shadow-zinc-800"
      >
        <div className="flex justify-center text-primary">
          <Image
            src={theme === "dark" ? "/logo-dark.png" : "/logo.png"}
            alt="logo"
            width={200}
            height={100}
            priority
            className="dark:bg-primary text-primary"
          />
        </div>
        <Menu isOpen={getOpenState()} />
      </div>
    </aside>
  );
}
