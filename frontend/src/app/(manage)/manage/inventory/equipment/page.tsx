import { ContentLayout } from "@/components/admin-panel/content-layout";
import EquipmentCardList from "@/features/equipment/components/equipment-card-list";

export default function Page() {
  return (
    <ContentLayout scrollable={false}>
      <EquipmentCardList />
    </ContentLayout>
  );
}
