import SupplierDetailLayout from "@/features/supplier/components/supplier-detail/supplier-detail-layout";

interface Props {
  params: { publicId: string }; // `params.id` chứa giá trị từ URL
}

export default async function Page(props: Props) {
  return <SupplierDetailLayout publicId={props.params.publicId} />;
}
