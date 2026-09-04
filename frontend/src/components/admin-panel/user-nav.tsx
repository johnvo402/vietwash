"use client";

import Link from "next/link";
import { Computer, LogOut, SquareGanttChart } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import Image from "next/image";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
  TooltipProvider,
} from "@/components/ui/tooltip";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { PersonIcon } from "@radix-ui/react-icons";
import { useRouter, useSearchParams } from "next/navigation";
import { usePageType } from "@/hooks/use-page-type";
import { useEffect, useState } from "react";
import { ProfileDialog } from "@/features/profile/profile-dialog";
import { useAuth } from "@/hooks/use-auth";
import { apiClient } from "@/api/client";
import {
  ROUTE_CASHIER,
  ROUTE_DASHBOARD,
  ROUTE_ORDERS,
  ROUTE_LOGIN,
} from "@/types/router-type";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { UserProfile } from "@/types/user";
import { ModeToggle } from "../mode-toggle";
import LanguageSwitcher from "../ui/language-switcher";
import { Gender, MediaType } from "@/api/generated/api";
import { useTranslations } from "next-intl";
import { toast } from "react-toastify";

export function UserNav() {
  const [viewProfile, setViewProfile] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const { isCashierPage } = usePageType();
  const router = useRouter();
  const searchParams = useSearchParams();
  const t = useTranslations();
  const { user, logout } = useAuth();
  const queryClient = useQueryClient();

  const updateProfileMutation = useMutation({
    mutationFn: async (data: { user: UserProfile; avt?: File }) => {
      let avtUrl = data.user.avtUrl;
      if (data.avt) {
        await apiClient
          .authApiMediaPost([data.avt], MediaType.Image)
          .then((response) => {
            avtUrl = response.data.results?.key?.[0] || null;
          });
      }
      return apiClient.authApiAccountsProfilePut({
        avtUrl: avtUrl,
        birthDay: data.user.birthDay,
        displayName: data.user.displayName,
        gender: data.user.gender as Gender,
        phoneNumber: data.user.phoneNumber,
        accountContact: data.user.accountContact,
        email: data.user.email,
        otpEmail: data.user.otpEmail,
        otpPhoneNumber: data.user.otpPhone,
      });
    },
    onSuccess: () => {
      toast.info(
        t("toast.update.success", {
          entity: t("user.profile"),
        }),
      );
      queryClient.invalidateQueries({ queryKey: ["me"] });
    },
    onError: (err) => {
      toast.error(t("toast.update.failed", { entity: t("user.profile") }));
    },
  });

  const changePasswordMutation = useMutation({
    mutationFn: async (data: { oldPassword: string; newPassword: string }) => {
      return apiClient.authApiAccountsChangePasswordPut({
        newPassword: data.newPassword,
        oldPassword: data.oldPassword,
      });
    },
    onSuccess: () => {
      toast.info(t("user.passChanged"));
    },
    onError: () => {
      toast.error(t("user.passChangeFailed"));
    },
  });

  const logoutHandler = async () => {
    try {
      await apiClient.authApiAccountsLogoutPost();
    } catch (error) {
      console.warn(
        "The server logout request failed; clearing the local session.",
        error,
      );
    }

    logout();
    queryClient.clear();
    router.replace(ROUTE_LOGIN);
  };

  useEffect(() => {
    const open = searchParams.get("nav-bar-open");
    if (open) {
      setDropdownOpen(true);
    }
  }, [router, searchParams]);

  return (
    <>
      <DropdownMenu open={dropdownOpen} onOpenChange={setDropdownOpen}>
        <TooltipProvider disableHoverableContent>
          <Tooltip delayDuration={100}>
            <TooltipTrigger asChild>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="outline"
                  className="relative h-8 w-8 rounded-full"
                >
                  <Avatar className="h-8 w-8">
                    {user?.avtUrl ? (
                      <Image
                        src={user.avtUrl}
                        alt="Avatar"
                        className="h-8 w-8 rounded-full object-cover"
                        fill
                        style={{ objectFit: "contain" }}
                      />
                    ) : (
                      <AvatarFallback className="bg-transparent">
                        {user?.displayName?.[0]?.toUpperCase()}
                      </AvatarFallback>
                    )}
                  </Avatar>
                </Button>
              </DropdownMenuTrigger>
            </TooltipTrigger>
            <TooltipContent side="bottom">
              {t("user.profile").charAt(0).toUpperCase() +
                t("user.profile").slice(1)}
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>

        <DropdownMenuContent className="w-56" align="end">
          <DropdownMenuLabel className="font-normal">
            <div className="flex flex-col space-y-1">
              <p className="text-sm font-medium leading-none">
                {user?.displayName}
              </p>
              <p className="text-xs leading-none text-muted-foreground">
                {user?.email}
              </p>
            </div>
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          {isCashierPage ? (
            <DropdownMenuGroup>
              <DropdownMenuItem className="hover:cursor-pointer" asChild>
                <Link
                  href={user?.role === "STAFF" ? ROUTE_ORDERS : ROUTE_DASHBOARD}
                  className="flex items-center"
                >
                  <SquareGanttChart className="w-4 h-4 mr-3 text-muted-foreground" />
                  {t("common.managerPage")}
                </Link>
              </DropdownMenuItem>
            </DropdownMenuGroup>
          ) : (
            <DropdownMenuGroup>
              <DropdownMenuItem className="hover:cursor-pointer" asChild>
                <Link href={ROUTE_CASHIER} className="flex items-center">
                  <Computer className="w-4 h-4 mr-3 text-muted-foreground" />
                  {t("common.cashierPage")}
                </Link>
              </DropdownMenuItem>
            </DropdownMenuGroup>
          )}
          <DropdownMenuGroup>
            <DropdownMenuItem
              className="hover:cursor-pointer"
              onClick={() => setViewProfile(true)}
              onSelect={(event) => {
                event.preventDefault(); // Ngăn dropdown đóng khi click vào Theme
              }}
            >
              <PersonIcon className="w-4 h-4 mr-3 text-muted-foreground" />
              {t("common.account")}
            </DropdownMenuItem>
          </DropdownMenuGroup>
          <DropdownMenuSeparator />
          <DropdownMenuGroup>
            <DropdownMenuItem
              onSelect={(event) => {
                event.preventDefault();
              }}
            >
              <ModeToggle />
              <LanguageSwitcher />
            </DropdownMenuItem>
          </DropdownMenuGroup>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            className="hover:cursor-pointer"
            onClick={() => logoutHandler()}
          >
            <LogOut className="w-4 h-4 mr-3 text-muted-foreground" />
            {t("common.sign_out")}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      {viewProfile && user && (
        <ProfileDialog
          user={user}
          visible={viewProfile}
          onClose={() => setViewProfile(false)}
          updateProfileMutation={updateProfileMutation}
          changePasswordMutation={changePasswordMutation}
        />
      )}
    </>
  );
}
