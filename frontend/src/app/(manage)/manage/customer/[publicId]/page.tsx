import CustomerDetailLayout from "@/features/customer/components/customer-detail/customer-detail-layout";

interface Props {
  params: Promise<{ publicId: string }>; // `params.id` chứa giá trị từ URL
}

export default async function Page(props: Props) {
  return <CustomerDetailLayout publicId={(await props.params).publicId} />;
}
