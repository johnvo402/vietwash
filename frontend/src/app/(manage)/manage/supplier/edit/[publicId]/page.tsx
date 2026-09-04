import SupplierPageUpdate from "@/features/supplier/supplier-update-page";
interface EditProps {
  params: Promise<{ publicId: string }>; // `params.id` chứa giá trị từ URL
}
const Page = async (props: EditProps) => {
  return <SupplierPageUpdate publicId={(await props.params).publicId} />;
};
export default Page;
