import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useTranslations } from "next-intl";
import { useQueryState } from "nuqs";

export const CustomerDetailTab = () => {
  const t = useTranslations();
  const tabItems = ["orders", "pointHistory"];

  const [tab, setTab] = useQueryState("tabCustomer", {
    defaultValue: "orders",
    parse: (value) => (tabItems.includes(value) ? value : "orders"),
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
