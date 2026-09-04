import DetailBranchProductPage from "@/features/inventories/materials/view/detail-material-view";
interface DetailProps {
  params: Promise<{
    publicId: string;
  }>;
}
export default async function Page({ params: paramsPromise }: DetailProps) {
  const params = await paramsPromise;
  return <DetailBranchProductPage params={params} />;
}
