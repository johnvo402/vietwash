"use client";
import { Tabs } from "@/components/ui/tabs";
import { TabsContent } from "@radix-ui/react-tabs";
import { useQueryState } from "nuqs";
import { UnitRelationProjection } from "@/api/generated/api";
import UnitListDisplay from "@/features/services/components/service-detail/content-section/unit-section";
import CarInvList from "./card-inventory";
interface UnitListDisplayProps {
  unitRelations?: UnitRelationProjection[];
  id: number;
}
export default function BranchProductDetailTabContent({
  unitRelations,
  id,
}: UnitListDisplayProps) {
  const [tab] = useQueryState("tabBranchProductDetail", {
    defaultValue: "unit_relation",
    parse: (value) =>
      ["unit_relation", "inventoryCard"].includes(value)
        ? value
        : "unit_relation",
  });
  return (
    <Tabs value={tab}>
      <TabsContent value="unit_relation">
        <UnitListDisplay unitRelations={unitRelations} />
      </TabsContent>
      <TabsContent value="inventoryCard">
        <CarInvList id={id} />
      </TabsContent>
    </Tabs>
  );
}
