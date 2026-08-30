import {
  Users,
  LayoutGrid,
  LucideIcon,
  Settings,
  Box,
  WalletCards,
  ShoppingBasket,
  ClipboardMinus,
  Database,
  StoreIcon,
  Contact,
  Container,
  Warehouse,
  Layers2,
  SquareArrowDownRight,
  SquareArrowDownLeft,
  Blocks,
  GiftIcon,
} from "lucide-react";
import {
  ROUTE_CUSTOMER,
  ROUTE_DASHBOARD,
  ROUTE_EQUIPMENT,
  ROUTE_FUND,
  ROUTE_INVENTORY,
  ROUTE_INVENTORY_EXPORT,
  ROUTE_INVENTORY_IMPORT,
  ROUTE_INVENTORY_MATERIAL,
  ROUTE_ORDERS,
  ROUTE_REPORT,
  ROUTE_SERVICE,
  ROUTE_SETTING_DATA,
  ROUTE_SETTING_SYSTEM,
  ROUTE_SETTINGS,
  ROUTE_SUPPLIER,
  ROUTE_USERS,
  ROUTE_VOUCHER,
} from "@/types/router-type";
import { useTranslations } from "next-intl";
import { useAuth } from "@/hooks/use-auth";
import { Role } from "@/utils/roles";

type Submenu = {
  href: string;
  label: string;
  icon?: LucideIcon;
  active?: boolean;
  roles: Role[];
};

type Menu = {
  href: string;
  label: string;
  active?: boolean;
  icon: LucideIcon;
  submenus?: Submenu[];
  roles: Role[];
};

export function useMenuList(): Menu[] {
  const t = useTranslations();
  const { user } = useAuth(); // Get user from useAuth hook
  const userRole = user?.role as Role; // Extract user role

  const menus: Menu[] = [
    {
      href: ROUTE_DASHBOARD,
      label: t(`route.${ROUTE_DASHBOARD}`),
      icon: LayoutGrid,
      roles: [Role.ADMIN, Role.MANAGER],
    },
    {
      href: ROUTE_REPORT,
      label: t(`route.${ROUTE_REPORT}`),
      icon: ClipboardMinus,
      roles: [Role.ADMIN, Role.MANAGER],
    },
    {
      href: ROUTE_FUND,
      label: t(`route.${ROUTE_FUND}`),
      icon: WalletCards,
      roles: [Role.ADMIN, Role.MANAGER],
    },
    {
      href: ROUTE_ORDERS,
      label: t(`route.${ROUTE_ORDERS}`),
      icon: ShoppingBasket,
      roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
    },
    {
      href: ROUTE_INVENTORY,
      label: t(`route.${ROUTE_INVENTORY}`),
      icon: Warehouse,
      roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
      submenus: [
        {
          href: ROUTE_INVENTORY_MATERIAL,
          label: t(`route.${ROUTE_INVENTORY_MATERIAL}`),
          icon: Layers2,
          roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
        },
        {
          href: ROUTE_EQUIPMENT,
          label: t(`route.${ROUTE_EQUIPMENT}`),
          icon: Blocks,
          roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
        },
        {
          href: ROUTE_INVENTORY_IMPORT,
          label: t(`route.${ROUTE_INVENTORY_IMPORT}`),
          icon: SquareArrowDownRight,
          roles: [Role.ADMIN, Role.MANAGER],
        },
        {
          href: ROUTE_INVENTORY_EXPORT,
          label: t(`route.${ROUTE_INVENTORY_EXPORT}`),
          icon: SquareArrowDownLeft,
          roles: [Role.ADMIN, Role.MANAGER],
        },
      ],
    },
    {
      href: ROUTE_SERVICE,
      label: t(`route.${ROUTE_SERVICE}`),
      icon: Box,
      roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
    },
    {
      href: ROUTE_SUPPLIER,
      label: t(`route.${ROUTE_SUPPLIER}`),
      icon: Container,
      roles: [Role.ADMIN, Role.MANAGER],
    },
    {
      href: ROUTE_CUSTOMER,
      label: t(`route.${ROUTE_CUSTOMER}`),
      icon: Contact,
      roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
    },
    {
      href: ROUTE_USERS,
      label: t(`route.${ROUTE_USERS}`),
      icon: Users,
      roles: [Role.ADMIN, Role.MANAGER],
    },
    {
      href: ROUTE_VOUCHER,
      label: t(`route.${ROUTE_VOUCHER}`),
      icon: GiftIcon,
      roles: [Role.ADMIN, Role.MANAGER, Role.STAFF],
    },
    {
      href: "",
      label: t(`route.${ROUTE_SETTINGS}`),
      icon: Settings,
      roles: [Role.ADMIN, Role.MANAGER],
      submenus: [
        {
          href: ROUTE_SETTING_DATA,
          label: t(`route.${ROUTE_SETTING_DATA}`),
          icon: Database,
          roles: [Role.ADMIN, Role.MANAGER],
        },
        {
          href: ROUTE_SETTING_SYSTEM,
          label: t(`route.${ROUTE_SETTING_SYSTEM}`),
          icon: StoreIcon,
          roles: [Role.ADMIN],
        },
      ],
    },
  ];

  // Filter menus based on user role
  if (!userRole) {
    return []; // Return empty array if no user role (user not logged in)
  }

  return menus
    .map((menu) => {
      // Check if menu has submenus
      if (menu.submenus) {
        // Filter submenus based on user role
        const filteredSubmenus = menu.submenus.filter((submenu) =>
          submenu.roles.includes(userRole)
        );
        // Only include menu if user has access to it or its submenus
        if (menu.roles.includes(userRole) || filteredSubmenus.length > 0) {
          return {
            ...menu,
            submenus: filteredSubmenus,
          };
        }
        return null;
      }
      // For menus without submenus, check if user has access
      return menu.roles.includes(userRole) ? menu : null;
    })
    .filter((menu): menu is Menu => menu !== null); // Remove null entries
}
