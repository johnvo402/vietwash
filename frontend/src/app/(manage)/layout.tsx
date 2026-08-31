import AdminPanelLayout from "@/components/admin-panel/admin-panel-layout";
import ProtectedRoute from "@/components/main/ProtectedRoute";
import { generateTranslatedMetadata } from "@/lib/metadata";
import { ROUTE_MANAGE } from "@/types/router-type";
export const generateMetadata = () =>
  generateTranslatedMetadata({
    pathname: ROUTE_MANAGE, // root pathname
  });
export default async function ManageLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <ProtectedRoute>
      <AdminPanelLayout>{children}</AdminPanelLayout>
    </ProtectedRoute>
  );
}
