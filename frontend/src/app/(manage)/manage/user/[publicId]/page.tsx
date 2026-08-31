import UserDetailLayout from "@/features/users/components/user-detail/user-detail-layout";

interface Props {
  params: { publicId: string }; // `params.id` chứa giá trị từ URL
}

export default async function Page(props: Props) {
  return <UserDetailLayout publicId={props.params.publicId} />;
}
