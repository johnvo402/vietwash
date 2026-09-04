import { ROUTE_CASHIER, ROUTE_DASHBOARD } from "@/types/router-type";

// A navigation hint only: endpoint authorization remains authoritative.
export function getLandingRoute(role: string | null | undefined): string {
  switch (role) {
    case "ADMIN":
    case "MANAGER":
      return ROUTE_DASHBOARD;
    case "STAFF":
      return ROUTE_CASHIER;
    default:
      return "/403";
  }
}
