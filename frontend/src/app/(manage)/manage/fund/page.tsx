import { ContentLayout } from "@/components/admin-panel/content-layout";
import { FundPageView } from "@/features/fund/fund-page-view";
import { searchParamsCache, serialize } from "@/lib/searchparams";
import { SearchParams } from "nuqs/server";

type pageProps = {
  searchParams: Promise<SearchParams>;
};
export default async function Page(props: pageProps) {
  const searchParams = await props.searchParams;
  // Allow nested RSCs to access the search params (in a type-safe way)
  searchParamsCache.parse(searchParams);
  const key = serialize({ ...searchParams });

  return (
    <ContentLayout key={key} scrollable={false}>
      <div className="flex flex-1 flex-col space-y-4 max-w-full">
        <FundPageView />
      </div>
    </ContentLayout>
  );
}
