import { ContentLayout } from "@/components/admin-panel/content-layout";
import SettingTab, {
  TabItem,
} from "@/features/settings/components/setting-tab";
import SettingInternalPageView from "@/features/settings/setting-internal/setting-page-view";
import { useTranslations } from "next-intl";

export default function Page() {
  const t = useTranslations();
  const tabItems: TabItem[] = [
    {
      tabId: "branch",
      title: t("common.branch").replace(/^./, (c) => c.toUpperCase()),
    },
    {
      tabId: "tariff",
      title: t("common.tariff").replace(/^./, (c) => c.toUpperCase()),
    },
  ];
  return (
    <div className="min-h-full ">
      <div className="flex flex-col md:flex-row h-full">
        <div className="w-full md:w-64 md:min-h-[calc(100dvh-124px)] flex-shrink-0">
          <SettingTab tabItems={tabItems} />
        </div>

        <div className="flex-1 min-h-full bg-background flex p-4">
          <SettingInternalPageView />
        </div>
      </div>
    </div>
  );
}
