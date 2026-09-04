import UserPageUpdate from "@/features/users/UserPageUpdate";
interface UserEditProps {
  params: Promise<{ publicId: string }>; // `params.id` chứa giá trị từ URL
}
const Page = async (props: UserEditProps) => {
  return <UserPageUpdate publicId={(await props.params).publicId} />;
};
export default Page;
