"use client";
import { Tabs } from "@/components/ui/tabs";
import { TabsContent } from "@radix-ui/react-tabs";
import { useQueryState } from "nuqs";
import BranchSettingListPage from "./branch/branch-list-view";
import TariffSettingListPage from "./tariff/tariff-list-view";

export default function SettingInternalPageView() {
  const [tab] = useQueryState("tab", {
    defaultValue: "branch",
    parse: (value) => (["branch", "tariff"].includes(value) ? value : "branch"),
  });
  return (
    <div className="flex flex-1 flex-col space-y-4">
      <Tabs value={tab}>
        <TabsContent value="branch">
          <BranchSettingListPage />
        </TabsContent>
        <TabsContent value="tariff">
          <TariffSettingListPage />
        </TabsContent>
      </Tabs>
    </div>
  );
}
