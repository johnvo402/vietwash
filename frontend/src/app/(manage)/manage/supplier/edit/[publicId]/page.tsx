import SupplierPageUpdate from "@/features/supplier/supplier-update-page";
interface EditProps {
  params: { publicId: string }; // `params.id` chứa giá trị từ URL
}
const Page = (props: EditProps) => {
  return <SupplierPageUpdate publicId={props.params.publicId} />;
};
export default Page;
