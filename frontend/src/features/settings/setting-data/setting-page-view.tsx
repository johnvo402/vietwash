"use client";
import { Tabs } from "@/components/ui/tabs";
import UnitSettingListPage from "./unit-settings/unit-setting-list";
import { TabsContent } from "@radix-ui/react-tabs";
import { useQueryState } from "nuqs";
import CategoryTree from "./category-settings/product-tree";

export default function SettingDataPageView() {
  const [tab] = useQueryState("tab", {
    defaultValue: "unit",
    parse: (value) => (["unit", "category"].includes(value) ? value : "unit"),
  });
  return (
    <div className="flex flex-1 flex-col space-y-4">
      <Tabs value={tab} className="bg-background">
        <TabsContent value="unit">
          <UnitSettingListPage />
        </TabsContent>
        <TabsContent value="category">
          <CategoryTree />
        </TabsContent>
      </Tabs>
    </div>
  );
}
