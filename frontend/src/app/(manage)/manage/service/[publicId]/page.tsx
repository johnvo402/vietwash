import ServiceDetailLayout from "@/features/services/components/service-detail/service-detail-layout";

interface ServiceIdProps {
  params: Promise<{ publicId: string }>; // `params.id` chứa giá trị từ URL
}

export default async function Page(props: ServiceIdProps) {
  return <ServiceDetailLayout publicId={(await props.params).publicId} />;
}
