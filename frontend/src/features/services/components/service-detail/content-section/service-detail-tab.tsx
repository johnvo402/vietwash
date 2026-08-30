import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";

export const ServiceDetailTab = () => {
  const t = useTranslations();
  const tabItems = ["unit_relation", "orders", "feedback"];

  const [tab, setTab] = useQueryState("tabServiceDetail", {
    defaultValue: "unit_relation",
    parse: (value) => (tabItems.includes(value) ? value : "unit_relation"),
  });
  const handleTabChange = (value: string) => {
    setTab(value);
  };
  return (
    <Tabs
      defaultValue="details"
      value={tab}
      onValueChange={handleTabChange}
      orientation="horizontal"
    >
      <TabsList className="mb-4 justify-start">
        {tabItems.map((item) => (
          <TabsTrigger key={item} value={item} className="mr-4">
            {t(`tab.${item}`)}
          </TabsTrigger>
        ))}
      </TabsList>
    </Tabs>
  );
};
