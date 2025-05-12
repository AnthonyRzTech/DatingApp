import { useState } from "react";
import { Tag, ViewProfileProps } from "@/app/interfaces";
import {
  Typography,
  Stack,
  Box,
  Chip,
  Button,
  CardActions,
} from "@mui/material";
import AddAPhotoIcon from "@mui/icons-material/AddAPhoto";
import EditRoundedIcon from "@mui/icons-material/EditRounded";
import dayjs from "dayjs";
import { EditProfile } from "./EditProfile";
import { ProfilePictures } from "@/components/ProfilePictures";
import { capitalize } from "@/utils/helpers";
import { tagChipColors } from "@/components/theme";
import { FameLinearProgress } from "@/components/FameLinearProgress";
import { OnlineStatus } from "@/components/OnlineStatus";
import { ReportedIndicator } from "@/components/ReportedStatus";
import { ReportButton } from "@/components/ReportButton";

export const ViewProfile = ({ user, tags, images, me }: ViewProfileProps) => {
  if (!user.dateOfBirth) {
    user.gender = undefined;
    user.preferences = undefined;
  }
  const [editMode, setEditMode] = useState(false);
  const [editPictures, setEditPictures] = useState(false);

  if (editMode || !user.dateOfBirth) {
    return (
      <EditProfile
        user={user}
        userTags={tags}
        setEditMode={() => setEditMode(false)}
      />
    );
  }
  if (editPictures) {
    return (
      <ProfilePictures
        images={images}
        userId={user.id}
        editPictures={true}
        setEditPictures={() => setEditPictures(false)}
      />
    );
  }

  return (
    <>
      <Stack spacing={3}>
        <Box
          sx={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            gap: 1,
          }}
        >
          <ProfilePictures images={images} editMode={false} />
          <Stack direction="row" alignItems="center" spacing={1}>
            <Typography variant="h3">
              {capitalize(user.firstName)} {capitalize(user.lastName)}
            </Typography>
            {!me && <ReportedIndicator user={user} />}
          </Stack>
          <Typography>@{user.username?.toLowerCase()}</Typography>
          {!me && <OnlineStatus user={user} />}

          <Typography>{user.bio || "No bio provided"}</Typography>
          {!me && user.id && <ReportButton userId={user.id} />}
          <Stack
            direction="row"
            spacing={1.5}
            flexWrap="wrap"
            sx={{
              rowGap: 0.5,
              mt: 2,
            }}
          >
            {tags && tags?.length > 0 ? (
              tags?.map((tag: Tag) => (
                <Chip
                  key={tag.id}
                  label={`#${tag.tag}`}
                  sx={{
                    bgcolor: tagChipColors[tag.id % tagChipColors.length],
                  }}
                />
              ))
            ) : (
              <Typography variant="body2" color="text.secondary">
                No interests added yet
              </Typography>
            )}
          </Stack>
        </Box>
        <FameLinearProgress variant="determinate" value={user?.fameRate ?? 0} />
        {me && (
          <Box>
            <Typography variant="subtitle2" color="text.secondary">
              Email
            </Typography>
            <Typography>{user.email?.toLowerCase()}</Typography>
          </Box>
        )}

        <Box>
          <Typography variant="subtitle2" color="text.secondary">
            Date of Birth
          </Typography>
          <Typography>
            {user.dateOfBirth
              ? dayjs(user.dateOfBirth).format("MMMM D, YYYY")
              : "Not specified"}
          </Typography>
        </Box>

        <Box>
          <Typography variant="subtitle2" color="text.secondary">
            Gender
          </Typography>
          <Typography sx={{ textTransform: "capitalize" }}>
            {user.gender ? user.gender : "Not specified"}
          </Typography>
        </Box>

        <Box>
          <Typography variant="subtitle2" color="text.secondary">
            Preferences
          </Typography>
          <Typography sx={{ textTransform: "capitalize" }}>
            {user.preferences &&
            user.preferences.length > 0 &&
            Array.isArray(user.preferences)
              ? user.preferences
                  .map((pref) => pref.charAt(0).toUpperCase() + pref.slice(1))
                  .join(", ")
              : "Not specified"}
          </Typography>
        </Box>
        {user.city && user.city.trim().length > 0 && (
          <Box>
            <Typography variant="subtitle2" color="text.secondary">
              Location
            </Typography>
            <Typography>{user.city}</Typography>
          </Box>
        )}
      </Stack>
      {me && (
        <CardActions sx={{ justifyContent: "flex-end", p: 2 }}>
          <Button
            variant="contained"
            startIcon={<EditRoundedIcon />}
            onClick={() => {
              setEditMode(true);
            }}
          >
            Edit Info
          </Button>
          <Button
            variant="contained"
            startIcon={<AddAPhotoIcon />}
            onClick={() => {
              setEditPictures(true);
            }}
          >
            Edit Photo
          </Button>
        </CardActions>
      )}
    </>
  );
};
