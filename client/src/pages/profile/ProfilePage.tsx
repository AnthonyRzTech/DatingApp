import { Layout } from "@/components/Layout";
import { Profile } from "./Profile";
// import { useAuth } from "@/utils/authContext";
import { useFetchCurrentUser } from "../browse/usersActions";

export const ProfilePage = () => {
  const {
    data: userData,
    isLoading,
    isError,
    isSuccess,
  } = useFetchCurrentUser();
  return (
    <Layout>
      <Profile
        me={true}
        user={userData}
        userIsLoading={isLoading}
        userIsError={isError}
        userIsSuccess={isSuccess}
      />
    </Layout>
  );
};
