import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";

export const BranchProductDetailTab = () => {
  const t = useTranslations();
  const [tab, setTab] = useQueryState("tabBranchProductDetail", {
    defaultValue: "unit_relation",
    parse: (value) =>
      ["unit_relation", "inventoryCard"].includes(value)
        ? value
        : "unit_relation",
  });
  const handleTabChange = (value: string) => {
    setTab(value);
  };
  const tabItems = ["unit_relation", "inventoryCard"];
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
