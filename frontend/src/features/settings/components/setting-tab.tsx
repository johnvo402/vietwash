"use client";
import { useState } from "react";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { useQueryState } from "nuqs";
import { useTranslations } from "next-intl";
interface SettingTabProps {
  tabItems: TabItem[];
}

export interface TabItem {
  title: string;
  tabId: string;
}
export default function SettingTab({ tabItems }: SettingTabProps) {
  const t = useTranslations();
  const [tab, setTab] = useQueryState("tab", {
    defaultValue: tabItems[0].tabId,
    parse: (value) =>
      tabItems.some((x) => x.tabId === value) ? value : tabItems[0].tabId,
  });

  const [searchTerm, setSearchTerm] = useState("");

  const handleTabChange = (value: string) => {
    setTab(value);
  };

  const filteredTabs = tabItems.filter((item) =>
    t(`common.${item.tabId}`)
      .toLowerCase()
      .includes(searchTerm.toLowerCase())
  );
  const placeholder = t("search.typeData");

  return (
    <div className="h-full border-r bg-background">
      <div className="p-4">
        <div className="relative">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-foreground" />
          <Input
            type="search"
            placeholder={placeholder}
            className="w-full pl-9"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <Tabs
        value={tab}
        onValueChange={handleTabChange}
        orientation="vertical"
        className="w-full h-full"
      >
        <TabsList className="items-start flex h-full w-full flex-col justify-start rounded-none bg-background p-0">
          {filteredTabs.length > 0 &&
            filteredTabs.map((item) => (
              <TabsTrigger
                key={item.tabId}
                value={item.tabId}
                className="justify-start text-foreground w-full rounded-none border-l-4 border-transparent px-4 py-3 text-left data-[state=active]:border-l-primary data-[state=active]:bg-secondary data-[state=active]:text-primary data-[state=active]:shadow-none"
              >
                {item.title}
              </TabsTrigger>
            ))}
        </TabsList>
      </Tabs>
    </div>
  );
}
