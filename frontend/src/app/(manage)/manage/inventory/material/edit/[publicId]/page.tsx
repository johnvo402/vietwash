import MaterialPageUpdate from "@/features/inventories/materials/view/edit-materia-view";
interface DetailProps {
  params: {
    publicId: string;
  };
}
export default function Page({ params }: DetailProps) {
  return <MaterialPageUpdate params={params} />;
}
