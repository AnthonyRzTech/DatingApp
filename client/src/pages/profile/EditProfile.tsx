import { useState } from "react";
import { UserUpdateForm } from "../../components/UserUpdateForm";
import {
  Typography,
  useMediaQuery,
  Box,
  Button,
  Stack,
  CardActions,
} from "@mui/material";
import { SelectChangeEvent } from "@mui/material/Select";
import { Tag, UserUpdateFormProps } from "@/app/interfaces";
import { EditProfileProps } from "@/app/interfaces";
import { theme } from "@components/theme";
// import { useAuth } from "@/utils/authContext";
import { useFetchAllTags } from "../browse/usersActions";
import LoadingCup from "@/components/LoadingCup/LoadingCup";

export const EditProfile = ({
  user,
  userTags,
  setEditMode,
}: EditProfileProps) => {
  const { data: tags, isLoading: tagsIsLoading } = useFetchAllTags();
  const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
  const [dateOfBirth, setDateOfBirth] = useState<string>("");
  const [interests, setInterests] = useState<Tag[]>(userTags ?? []);

  if (tagsIsLoading) {
    return <LoadingCup />;
  }

  const handleChangeTags = (event: SelectChangeEvent<string[]>) => {
    const {
      target: { value },
    } = event;
    setInterests(tags?.filter((tag) => value.includes(tag.tag)) || []);
  };

  const formProps: UserUpdateFormProps = {
    user: user,
    tags: tags,
    userTags: interests,
    oldTags: userTags,
    onDateChange: setDateOfBirth,
    onTagsChange: handleChangeTags,
  };

  return (
    <>
      <Box
        sx={{
          display: "flex",
          flexDirection: isMobile ? "column" : "row",
          alignItems: "flex-start",
          justifyContent: "space-evenly",
          gap: 3,
          "& > *": {
            maxWidth: isMobile ? "100%" : "50%",
            width: "100%",
          },
        }}
      >
        <Stack spacing={3}>
          <Typography variant="h4">Personal Info</Typography>
          <UserUpdateForm {...formProps} />
        </Stack>
      </Box>
      <CardActions sx={{ justifyContent: "center" }}>
        <Button variant="outlined" onClick={setEditMode}>
          Back to Profile
        </Button>
      </CardActions>
    </>
  );
};
