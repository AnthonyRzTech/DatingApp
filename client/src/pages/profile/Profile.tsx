import { ViewProfile } from "./ViewProfile";
import { Card, CardContent } from "@mui/material";
import { useFetchUserTags } from "../browse/usersActions";
import { useFetchUserImages } from "../browse/usersActions";
import { User } from "@/app/interfaces";
import LoadingCup from "@/components/LoadingCup/LoadingCup";

interface ProfileProps {
  me: boolean;
  user?: User;
  userIsLoading?: boolean;
  userIsError?: boolean;
  userIsSuccess?: boolean;
}

export const Profile = ({
  me,
  user,
  userIsLoading,
  userIsSuccess,
}: ProfileProps) => {
  const { data: tags } = useFetchUserTags(user?.id);
  const {
    data: images,
    isLoading: imagesIsLoading,
    // isError: imagesIsError,
    isSuccess: imagesIsSuccess,
  } = useFetchUserImages(user?.id);
  let content;
  if (userIsLoading || imagesIsLoading) {
    content = <LoadingCup />;
  }
  if (userIsSuccess && user && images && imagesIsSuccess) {
    content = <ViewProfile tags={tags} user={user} images={images} me={me} />;
  }
  return (
    <>
      <Card>
        <CardContent sx={{ p: 5 }}>{content}</CardContent>
      </Card>
    </>
  );
};
