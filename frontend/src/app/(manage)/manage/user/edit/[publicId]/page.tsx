import UserPageUpdate from "@/features/users/UserPageUpdate";
interface UserEditProps {
  params: { publicId: string }; // `params.id` chứa giá trị từ URL
}
const Page = (props: UserEditProps) => {
  return <UserPageUpdate publicId={props.params.publicId} />;
};
export default Page;
