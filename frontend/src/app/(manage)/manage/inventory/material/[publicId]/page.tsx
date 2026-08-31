import DetailBranchProductPage from "@/features/inventories/materials/view/detail-material-view";
interface DetailProps {
  params: {
    publicId: string;
  };
}
export default function Page({ params }: DetailProps) {
  return <DetailBranchProductPage params={params} />;
}
