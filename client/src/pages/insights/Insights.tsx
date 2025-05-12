import { Layout } from "@/components/Layout";
import { Profile } from "../profile/Profile";
import {
  Avatar,
  Box,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  Popover,
  Skeleton,
  Stack,
  Typography,
  useTheme,
} from "@mui/material";
import VisibilityIcon from "@mui/icons-material/Visibility";
import FavoriteIcon from "@mui/icons-material/Favorite";
import { ReactElement, useState } from "react";
// import { useAuth } from "@/utils/authContext";
import { useLikes, useViews, useFetchUserById } from "./insightsHooks";
import LoadingCup from "@/components/LoadingCup/LoadingCup";
import { UserLike, UserView } from "@/app/interfaces";
import { useFetchCurrentUser } from "../browse/usersActions";

// Future improvements:
// - Add a "Load more" button to fetch more likes/views when there are too many
// - Use profile images instead of icons in the list

const format = (date: Date) => {
  // Time ago format
  const timeAgo = new Date(date).getTime();
  const now = new Date().getTime();
  const diff = now - timeAgo;
  const seconds = diff / 1000;
  const minutes = seconds / 60;
  const hours = minutes / 60;
  const days = hours / 24;
  const months = days / 30;
  const years = months / 12;

  if (seconds < 60) {
    return `${Math.floor(seconds)} seconds ago`;
  }
  if (minutes < 60) {
    return `${Math.floor(minutes)} minutes ago`;
  }
  if (hours < 24) {
    return `${Math.floor(hours)} hours ago`;
  }
  if (days < 30) {
    return `${Math.floor(days)} days ago`;
  }
  if (months < 12) {
    return `${Math.floor(months)} months ago`;
  }
  return `${Math.floor(years)} years ago`;
};

interface InsightsListProps {
  Icon: ReactElement;
  ListItems: UserLike[] | UserView[];
}

const InsightsItem = ({
  Icon,
  item,
  userId,
}: {
  Icon: ReactElement;
  item: UserLike | UserView;
  userId: number;
}) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const {
    data: user,
    isLoading: userIsLoading,
    isSuccess: userIsSuccess,
    isError: userIsError,
  } = useFetchUserById(userId);
  const theme = useTheme();

  // Event handlers
  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  // Render

  if (userIsLoading) {
    return (
      <Stack direction="row" alignItems="center" spacing={2} padding={1}>
        <Skeleton variant="circular" width={40} height={40} />
        <Skeleton variant="rectangular" width={100} height={10} />
      </Stack>
    );
  }

  if (userIsError) {
    return (
      <>
        <Typography>Error fetching user</Typography>
      </>
    );
  }

  if (userIsSuccess) {
    return (
      <>
        <ListItem
          key={user.id}
          onClick={handleClick}
          sx={{
            cursor: "pointer",
            "&:hover": {
              backgroundColor: theme.palette.action.hover,
            },
          }}
        >
          <ListItemAvatar>
            <Avatar>{Icon}</Avatar>
          </ListItemAvatar>
          <ListItemText
            primary={
              user.username.charAt(0).toUpperCase() + user.username.slice(1)
            }
            secondary={
              "likedAt" in item && item.likedAt instanceof Date
                ? format(item.likedAt)
                : "viewedAt" in item && item.viewedAt instanceof Date
                ? format(item.viewedAt)
                : "No date available"
            }
          ></ListItemText>
        </ListItem>

        <Popover
          open={Boolean(anchorEl)}
          anchorEl={anchorEl}
          onClose={() => setAnchorEl(null)}
          anchorOrigin={{
            vertical: "bottom",
            horizontal: "center",
          }}
          transformOrigin={{
            vertical: "top",
            horizontal: "center",
          }}
        >
          <Profile me={false} user={user} userIsSuccess={true} />
        </Popover>
      </>
    );
  }
  return <></>;
};

const InsightsList = ({ Icon, ListItems }: InsightsListProps) => {
  if (ListItems === undefined || ListItems.length === 0) {
    return <Typography>No data available</Typography>;
  }
  if ((ListItems[0] as UserLike).likedAt !== undefined) {
    ListItems = ListItems as UserLike[];
  } else {
    ListItems = ListItems as UserView[];
  }

  return (
    <List>
      {ListItems.map((item: UserLike | UserView) => (
        <InsightsItem
          Icon={Icon}
          item={item}
          userId={"likerId" in item ? item.likerId : item.viewerId}
          key={item.id}
        />
      ))}
    </List>
  );
};

export const Insights = () => {
  const {
    data: userData,
    isLoading: userDataLoading,
    isSuccess: userDataSuccess,
    isError: userDataError,
  } = useFetchCurrentUser();
  const {
    data: likes,
    isLoading: likesLoading,
    isSuccess: likesSuccess,
    error: likesErrors,
  } = useLikes(userData?.id);
  const {
    data: views,
    // isLoading: viewsLoading,
    isSuccess: viewsSuccess,
    // error: viewsErrors,
  } = useViews(userData?.id);

  /* _____________________________ Render ____________________________ */
  let content;
  if (userDataLoading || likesLoading) {
    content = <LoadingCup />;
  }

  if (likesErrors || userDataError) {
    content = (
      <>
        <Typography>Error fetching data</Typography>
      </>
    );
  }

  if (userDataSuccess && likesSuccess && viewsSuccess) {
    content = (
      <>
        <Paper
          elevation={3}
          sx={{ padding: "16px", width: { xs: "90%", md: "auto" } }}
        >
          <Typography variant="h4" sx={{ textAlign: "center" }}>
            Likes
          </Typography>
          <InsightsList Icon={<FavoriteIcon />} ListItems={likes} />
        </Paper>
        <Paper
          elevation={3}
          sx={{ padding: "16px", width: { xs: "90%", md: "auto" } }}
        >
          <Typography variant="h4" sx={{ textAlign: "center" }}>
            Views
          </Typography>
          <InsightsList Icon={<VisibilityIcon />} ListItems={views} />
        </Paper>
      </>
    );
  }

  return (
    <Layout>
      <Box
        sx={{
          display: "flex",
          flexDirection: { xs: "column", md: "row" }, // Column on mobile, row on desktop
          justifyContent: "center",
          alignItems: {
            xs: "center",
            md: "flex-start",
          },
          height: "100%",
          marginTop: "24px",
          gap: {
            xs: "24px", // Vertical gap for mobile
            md: "10vw", // Horizontal gap for desktop
          },
        }}
      >
        {content}
      </Box>
    </Layout>
  );
};
