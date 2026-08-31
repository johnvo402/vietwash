import { ContentLayout } from "@/components/admin-panel/content-layout";
import { Separator } from "@/components/ui/separator";
import ServiceListAction from "@/features/services/service-list-action";
import ServiceListingPage from "@/features/services/service-page-list";
import { searchParamsCache, serialize } from "@/lib/searchparams";
import { SearchParams } from "nuqs/server";
import { Suspense } from "react";
type pageProps = {
  searchParams: Promise<SearchParams>;
};
export default async function Page(props: pageProps) {
  const searchParams = await props.searchParams;
  // Allow nested RSCs to access the search params (in a type-safe way)
  searchParamsCache.parse(searchParams);

  const key = serialize({ ...searchParams });

  return (
    <ContentLayout scrollable={false}>
      <div className="flex flex-1 flex-col space-y-4">
        <ServiceListAction />
        <Suspense key={key} fallback={"Loading..."}>
          <ServiceListingPage />
        </Suspense>
      </div>
    </ContentLayout>
  );
}
