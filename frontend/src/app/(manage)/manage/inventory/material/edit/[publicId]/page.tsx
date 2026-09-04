import MaterialPageUpdate from "@/features/inventories/materials/view/edit-materia-view";
interface DetailProps {
  params: Promise<{
    publicId: string;
  }>;
}
export default async function Page({ params: paramsPromise }: DetailProps) {
  const params = await paramsPromise;
  return <MaterialPageUpdate params={params} />;
}
