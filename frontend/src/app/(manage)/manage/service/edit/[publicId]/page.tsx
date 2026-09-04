import ServicePageUpdate from "@/features/services/service-update-page";
interface ServiceEditProps {
  params: Promise<{ publicId: string }>; // `params.id` chứa giá trị từ URL
}

export default async function Page(props: ServiceEditProps) {
  return <ServicePageUpdate publicId={(await props.params).publicId} />;
}
