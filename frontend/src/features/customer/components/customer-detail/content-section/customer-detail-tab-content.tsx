"use client";
import { Tabs } from "@/components/ui/tabs";
import { TabsContent } from "@radix-ui/react-tabs";
import { useQueryState } from "nuqs";
import CustomerOrderView from "./order-customer";
import CustomerTransactionPage from "./customer-transaction";
interface UnitListDisplayProps {
  id: number;
}
export default function CustomerDetailTabContent({ id }: UnitListDisplayProps) {
  const [tab] = useQueryState("tabCustomer", {
    defaultValue: "orders",
    parse: (value) =>
      ["orders", "pointHistory"].includes(value) ? value : "orders",
  });
  return (
    <Tabs value={tab}>
      <TabsContent value="orders">
        <CustomerOrderView customerId={id} />
      </TabsContent>
      <TabsContent value="pointHistory">
        <CustomerTransactionPage id={id} />
      </TabsContent>
    </Tabs>
  );
}
