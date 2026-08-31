import SettingTab, {
  TabItem,
} from "@/features/settings/components/setting-tab";
import SettingDataPageView from "@/features/settings/setting-data/setting-page-view";
import { useTranslations } from "next-intl";

export default function Page() {
  const t = useTranslations();
  const tabItems: TabItem[] = [
    {
      tabId: "unit",
      title: t("common.unit").replace(/^./, (c) => c.toUpperCase()),
    },
    {
      tabId: "category",
      title: t("common.category").replace(/^./, (c) => c.toUpperCase()),
    },
  ];
  return (
    <div className="min-h-full">
      <div className="flex flex-col md:flex-row h-full">
        <div className="w-full md:w-64 md:min-h-[calc(100dvh-124px)] flex-shrink-0">
          <SettingTab tabItems={tabItems} />
        </div>

        <div className="flex-1 min-h-full">
          <SettingDataPageView />
        </div>
      </div>
    </div>
  );
}
