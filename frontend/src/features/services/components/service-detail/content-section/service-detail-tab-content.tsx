"use client";
import { Tabs } from "@/components/ui/tabs";
import { TabsContent } from "@radix-ui/react-tabs";
import { useQueryState } from "nuqs";
import UnitListDisplay from "./unit-section";
import { UnitRelationProjection } from "@/api/generated/api";
import ServiceOrderView from "./order-service";
import StaffReviewsComponent from "./feeback-service";
interface UnitListDisplayProps {
  unitRelations?: UnitRelationProjection[];
  id: number;
}
export default function ServiceDetailTabContent({
  unitRelations,
  id,
}: UnitListDisplayProps) {
  const [tab] = useQueryState("tabServiceDetail", {
    defaultValue: "unit_relation",
    parse: (value) =>
      ["unit_relation", "orders", "feedback"].includes(value)
        ? value
        : "unit_relation",
  });
  return (
    <Tabs value={tab}>
      <TabsContent value="unit_relation">
        <UnitListDisplay unitRelations={unitRelations} />
      </TabsContent>
      <TabsContent value="orders">
        <ServiceOrderView serviceId={id} />
      </TabsContent>
      <TabsContent value="feedback">
        <StaffReviewsComponent serviceId={id} />
      </TabsContent>
    </Tabs>
  );
}
