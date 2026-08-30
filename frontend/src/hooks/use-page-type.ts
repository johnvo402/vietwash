import { usePathname } from "next/navigation";

export function usePageType() {
  const pathname = usePathname();

  const isCashierPage = pathname.includes("cashier");
  const isManagerPage = pathname.includes("manage");

  return { isCashierPage, isManagerPage };
}
