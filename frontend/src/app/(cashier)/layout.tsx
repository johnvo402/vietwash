import { Toaster } from "@/components/ui/toaster";
import CashierLayout from "@/features/cashier/cashier-layout";
import { generateTranslatedMetadata } from "@/lib/metadata";
import { ROUTE_CASHIER } from "@/types/router-type";
export const generateMetadata = () =>
  generateTranslatedMetadata({
    pathname: ROUTE_CASHIER, // root pathname
  });
export default async function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <CashierLayout>
      {children} <Toaster />
    </CashierLayout>
  );
}
