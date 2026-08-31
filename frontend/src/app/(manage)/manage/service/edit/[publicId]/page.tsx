import ServicePageUpdate from "@/features/services/service-update-page";
interface ServiceEditProps {
  params: { publicId: string }; // `params.id` chứa giá trị từ URL
}

export default async function Page(props: ServiceEditProps) {
  return <ServicePageUpdate publicId={props.params.publicId} />;
}
