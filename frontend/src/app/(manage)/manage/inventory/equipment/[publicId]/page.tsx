import DetailEquipmentPage from "@/features/equipment/views/equipment-detail-view";
interface DetailProps {
  params: {
    publicId: string;
  };
}
export default function Page({ params }: DetailProps) {
  return <DetailEquipmentPage params={params} />;
}
