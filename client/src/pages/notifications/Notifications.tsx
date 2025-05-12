import { Layout } from "@/components/Layout";
import NotificationsIcon from "@mui/icons-material/Notifications";
import LoadingCup from "@/components/LoadingCup/LoadingCup";
import {
  Typography,
  Box,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Paper,
  IconButton,
  Button,
} from "@mui/material";
import { useFetchCurrentUser } from "../browse/usersActions";
import { NotificationInterface } from "@/utils/socket";
import { usePayload } from "@/utils/payloadProvider";
import {
  Delete,
  NotificationsActive,
  NotificationsNone,
} from "@mui/icons-material";

export const Notifications = () => {
  const {
    // data: userData,
    isSuccess: userIsSuccess,
    isLoading: userIsLoading,
  } = useFetchCurrentUser();
  const {
    notifications,
    notifMarkAsRead,
    notifMarkAsUnread,
    notifDelete,
    notifClear,
  } = usePayload();

  /* ______________________________ Render ______________________________ */
  let content;

  if (userIsLoading) {
    content = <LoadingCup />;
  } else if (notifications && userIsSuccess) {
    content = (
      <>
        <Paper
          elevation={3}
          sx={{ padding: "16px", width: { xs: "90%", md: "auto" } }}
        >
          <Typography variant="h4" textAlign={"center"}>
            Notifications
          </Typography>
          <Box display="flex" justifyContent="center" marginBottom="16px">
            <Button onClick={() => notifClear()} variant="contained">
              <NotificationsNone />
              Clear all
            </Button>
          </Box>
          <List dense={true}>
            {notifications.map((notification: NotificationInterface) => (
              <ListItem
                key={notification.id}
                secondaryAction={
                  <IconButton
                    edge="end"
                    aria-label="delete"
                    onClick={() => notifDelete(notification.id)}
                  >
                    {/* Change color based on read status */}
                    {notification.isRead ? (
                      <Delete color="disabled" />
                    ) : (
                      <Delete
                        color={
                          notification.ui_variant !== "default"
                            ? notification.ui_variant
                            : "secondary"
                        }
                      />
                    )}
                  </IconButton>
                }
              >
                <ListItemAvatar>
                  <IconButton
                    onClick={() => {
                      notification.isRead
                        ? notifMarkAsUnread(notification.id)
                        : notifMarkAsRead(notification.id);
                    }}
                  >
                    {/* Change color based on read status */}
                    {notification.isRead ? (
                      <NotificationsIcon color="disabled" />
                    ) : (
                      <NotificationsActive
                        color={
                          notification.ui_variant !== "default"
                            ? notification.ui_variant
                            : "secondary"
                        }
                      />
                    )}
                  </IconButton>
                </ListItemAvatar>
                <ListItemText primary={notification.content.message} />
              </ListItem>
            ))}
          </List>
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
