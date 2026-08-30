"use client";

import Link from "next/link";
// import { LogOut } from "lucide-react";
// import { useRouter } from "next/navigation";
// import { useAuth } from "@/hooks/use-auth";
// import { ROUTE_LOGIN } from "@/types/router-type";
// import { apiClient } from "@/api/client";
// import { toast } from "react-toastify";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { useMenuList } from "@/lib/menu-list";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { CollapseMenuButton } from "@/components/admin-panel/collapse-menu-button";
import {
  Tooltip,
  TooltipTrigger,
  TooltipContent,
  TooltipProvider,
} from "@/components/ui/tooltip";

interface MenuProps {
  isOpen: boolean | undefined;
}

export function Menu({ isOpen }: MenuProps) {
  // const { logout } = useAuth.getState();
  // const router = useRouter();
  const pathname = usePathname();
  const menuList = useMenuList();
  // const logoutHandler = async () => {
  //   const res = await apiClient.authApiAccountsLogoutPost();
  //   logout();
  //   toast.info(res.data.results?.message);
  //   router.replace(ROUTE_LOGIN);
  // };
  return (
    <ScrollArea className="[&>div>div[style]]:!block">
      <nav className="mt-8 h-full w-full">
        <ul className="flex flex-col min-h-[calc(100vh-48px-36px-16px-32px)] lg:min-h-[calc(100vh-32px-40px-32px)] items-start space-y-1 px-2">
          {menuList.map(
            ({ href, label, icon: Icon, active, submenus }, index) => (
              <li className={cn("w-full")} key={index}>
                {!submenus || submenus.length === 0 ? (
                  <div className="w-full">
                    <TooltipProvider disableHoverableContent>
                      <Tooltip delayDuration={100}>
                        <TooltipTrigger asChild>
                          <Button
                            variant={
                              (active === undefined &&
                                pathname.startsWith(href)) ||
                              active
                                ? "default"
                                : "ghost"
                            }
                            className={cn(
                              "w-full  h-10 mb-1",
                              !isOpen ? "justify-center" : "justify-start"
                            )}
                            asChild
                          >
                            <Link href={href}>
                              <span
                                className={cn(isOpen === false ? "" : "mr-2")}
                              >
                                <Icon size={18} />
                              </span>
                              {isOpen && (
                                <p
                                  className={cn(
                                    "max-w-[200px] truncate",
                                    !isOpen
                                      ? "-translate-x-96 opacity-0"
                                      : "translate-x-0 opacity-100"
                                  )}
                                >
                                  {label}
                                </p>
                              )}
                            </Link>
                          </Button>
                        </TooltipTrigger>
                        {isOpen === false && (
                          <TooltipContent side="right">{label}</TooltipContent>
                        )}
                      </Tooltip>
                    </TooltipProvider>
                  </div>
                ) : (
                  <div className="w-full">
                    <CollapseMenuButton
                      icon={Icon}
                      label={label}
                      active={
                        active === undefined
                          ? pathname.startsWith(href)
                          : active
                      }
                      submenus={submenus}
                      isOpen={isOpen}
                    />
                  </div>
                )}
              </li>
            )
          )}

          {/* <li className="w-full grow flex items-end pb-4">
            <TooltipProvider disableHoverableContent>
              <Tooltip delayDuration={100}>
                <TooltipTrigger asChild>
                  <Button
                    onClick={logoutHandler}
                    variant="outline"
                    className="w-full justify-center h-10 mt-5"
                  >
                    <span className={cn(isOpen === false ? "" : "mr-4")}>
                      <LogOut size={18} />
                    </span>
                    <p
                      className={cn(
                        "whitespace-nowrap",
                        isOpen === false ? "opacity-0 hidden" : "opacity-100"
                      )}
                    >
                      {t("common.sign_out")}
                    </p>
                  </Button>
                </TooltipTrigger>
                {isOpen === false && (
                  <TooltipContent side="right">
                    {t("common.sign_out")}
                  </TooltipContent>
                )}
              </Tooltip>
            </TooltipProvider>
          </li> */}
        </ul>
      </nav>
    </ScrollArea>
  );
}
