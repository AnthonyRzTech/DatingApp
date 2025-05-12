import {
  Card,
  CardContent,
  Typography,
  CardMedia,
  Box,
  Skeleton,
  IconButton,
  Popover,
  Button,
} from "@mui/material";
import { UserCardProps } from "@app/interfaces";
import {
  useBlockUser,
  useFetchUserProfilePic,
} from "@/pages/browse/usersActions";
import { Favorite, FavoriteBorder, PersonRemove } from "@mui/icons-material";
import { client } from "@/utils/axios";
import { useMutation, useQuery } from "@tanstack/react-query";
import LoadingCup from "./LoadingCup/LoadingCup";
import { useState } from "react";
import { Profile } from "@/pages/profile/Profile";
import { grey, red } from "@mui/material/colors";
import { theme } from "./theme";
import BlockIcon from "@mui/icons-material/Block";
import { useQueryClient } from "@tanstack/react-query";
import { closeSnackbar, useSnackbar } from "notistack";

export const UserCard = ({ user, match }: UserCardProps) => {
  const queryClient = useQueryClient();
  const { enqueueSnackbar } = useSnackbar();

  const { data: profilePic, isLoading: profilePicIsLoading } =
    useFetchUserProfilePic(user.id);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [subButtonClicked, setSubButtonClicked] = useState(false);
  // Add this near other state declarations
  const [isBlockClicked, setIsBlockClicked] = useState(false);
  // Check if user is liked
  const { data: isUserLiked, refetch: refetchLikeStatus } = useQuery({
    queryKey: ["isUserLiked", user.id],
    queryFn: async () => {
      const response = await client.get(`/users/${user.id}/liked/`);
      const data = {
        liked: response.data.data.isLiked,
      };
      if (data.liked === true) {
        return true;
      }
      return false;
    },
  });

  // Like/Unlike mutation
  const { mutate: toggleLike, isPending: isLikeLoading } = useMutation({
    mutationFn: async () => {
      if (isUserLiked) {
        return client.delete(`/users/${user.id}/likes/`);
      }
      return client.post(`/users/${user.id}/likes/`);
    },
    onSuccess: () => {
      refetchLikeStatus();
    },
  });

  const handleLike = async () => {
    setSubButtonClicked(true);
    if (!isLikeLoading) {
      toggleLike();
    }
  };

  const { mutate: removeMatch } = useMutation({
    mutationFn: async () => {
      await client.delete(`/users/${user.id}/matches/`);
    },
    onSuccess: () => {
      console.log("Match removed successfully");

      // Invalidate the matches query to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ["matches"] });
    },
    onError: (error) => {
      console.error("Error removing match:", error);
    },
  });
  const handleMatchRemove = async () => {
    setSubButtonClicked(true);
    console.log("Removing match");
    if (window.confirm("Are you sure you want to remove this match?")) {
      removeMatch();
    }
  };

  const blockUser = useBlockUser();

  if (profilePicIsLoading) {
    return (
      <Card
        sx={{
          maxWidth: 345,
          height: {
            xs: "auto",
            sm: 360,
          },
          margin: 2,
        }}
      >
        {/* <Skeleton variant="rectangular" height={200} animation="wave" /> */}
        <LoadingCup isMainLoading={false} />
        <CardContent>
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <Skeleton variant="text" width={100} animation="wave" />
            <Skeleton variant="text" width={40} animation="wave" />
          </Box>
          <Skeleton variant="text" width={150} animation="wave" />
        </CardContent>
      </Card>
    );
  }
  const open = Boolean(anchorEl);
  const id = open ? "profile-popover" : undefined;

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    if (subButtonClicked) {
      setSubButtonClicked(false);
      return;
    }
    try {
      client.post(`/users/${user.id}/views/`);
    } catch (error) {
      console.error(error);
    }
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleBlock = (event: React.MouseEvent<HTMLElement>) => {
    event.stopPropagation();
    setIsBlockClicked(true);
    enqueueSnackbar(`Are you sure you want to block ${user.username}?`, {
      variant: "error",
      autoHideDuration: null,
      action: (key) => (
        <>
          <Button
            color="inherit"
            size="small"
            onClick={() => {
              blockUser(user.id);
              handleClose();
              closeSnackbar(key);
              setIsBlockClicked(false);
            }}
          >
            Yes
          </Button>
          <Button
            color="inherit"
            size="small"
            onClick={() => {
              closeSnackbar(key);
              setIsBlockClicked(false);
            }}
          >
            No
          </Button>
        </>
      ),
      onClose: () => setIsBlockClicked(false),
    });
  };

  return (
    <>
      <Box
        onClick={handleClick}
        sx={{
          cursor: "pointer",
          "&:hover": { opacity: 0.9 },
        }}
      >
        <Card
          aria-describedby={id}
          sx={{
            maxWidth: 345,
            height: {
              xs: "auto",
              sm: 360,
            },
            position: "relative",
            transition: "transform 0.2s",
            "&:hover": {
              transform: "scale(1.02)",
              cursor: "pointer",
            },
            borderWidth: 10,
            borderColor: isBlockClicked ? red[500] : theme.palette.primary.main,
            borderStyle: "solid",
            borderRadius: 5,
            boxShadow: 3,
          }}
        >
          {match ? (
            <IconButton
              sx={{
                position: "absolute",
                top: 8,
                right: 8,
                backgroundColor: grey[50],
                "&:hover": {
                  backgroundColor: grey[300],
                },
                zIndex: 1,
              }}
              aria-label="Remove Match"
              color="error"
              onMouseDown={handleMatchRemove}
            >
              <PersonRemove />
            </IconButton>
          ) : (
            <IconButton
              sx={{
                position: "absolute",
                top: 8,
                right: 8,
                backgroundColor: grey[50],
                "&:hover": {
                  backgroundColor: grey[300],
                },
                zIndex: 1,
              }}
              aria-label="Like"
              color="primary"
              onMouseDown={handleLike}
            >
              {isUserLiked ? <Favorite /> : <FavoriteBorder />}
            </IconButton>
          )}
          <IconButton
            sx={{
              position: "absolute",
              top: 8,
              left: 8,
              backgroundColor: grey[50],
              "&:hover": {
                backgroundColor: grey[300],
              },
              color: red[500],
              zIndex: 1,
            }}
            onClick={handleBlock}
          >
            <BlockIcon />
          </IconButton>
          <CardMedia
            component="img"
            image={
              profilePic?.url ||
              "https://images.unsplash.com/photo-1515823064-d6e0c04616a7?q=80&w=2071&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            }
            alt={`${user.username}'s profile picture`}
            sx={{
              objectFit: "cover",
              borderTopLeftRadius: 5,
              borderTopRightRadius: 5,
              backgroundColor: theme.palette.primary.main,
              width: "100%",
              height: 250,
            }}
          />
          <CardContent>
            <Box
              sx={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                minHeight: "50px",
              }}
            >
              <Typography variant="h6" sx={{ textTransform: "capitalize" }}>
                {user.username.length > 12
                  ? `${user.username.slice(0, 12)}...`
                  : user.username}
              </Typography>
              <Typography variant="body1">{user.age} years</Typography>
            </Box>
            {user.city && (
              <Typography variant="body2" color="text.secondary">
                {user.city}
              </Typography>
            )}
          </CardContent>
        </Card>
      </Box>

      <Popover
        id={id}
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{
          vertical: "center",
          horizontal: "center",
        }}
        transformOrigin={{
          vertical: "center",
          horizontal: "center",
        }}
      >
        <Profile me={false} user={user} userIsSuccess={true} />
      </Popover>
    </>
  );
};
