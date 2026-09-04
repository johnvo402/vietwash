import DetailEquipmentPage from "@/features/equipment/views/equipment-detail-view";
interface DetailProps {
  params: Promise<{
    publicId: string;
  }>;
}
export default async function Page({ params: paramsPromise }: DetailProps) {
  const params = await paramsPromise;
  return <DetailEquipmentPage params={params} />;
}
