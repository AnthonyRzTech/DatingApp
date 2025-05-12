import Divider from "@mui/material/Divider";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import ListItemIcon from "@mui/material/ListItemIcon";
import ChatIcon from "@mui/icons-material/Chat";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import SearchIcon from "@mui/icons-material/Search";
import FavoriteIcon from "@mui/icons-material/Favorite";
import LogoutIcon from "@mui/icons-material/Logout";
import NotificationsIcon from "@mui/icons-material/Notifications";
import InsightsIcon from "@mui/icons-material/Insights";
import { MatchaNavBar } from "./MatchaNavBar";
import Box from "@mui/material/Box";
import { useTheme, useMediaQuery, Badge } from "@mui/material";
import { useNavigate, useLocation } from "react-router-dom";
import { routes } from "@/utils/routes";
import { useLogout } from "@/pages/auth/authActions";
// import { useAuth } from "@/utils/authContext";
import { theme } from "./theme";
import {
  useFetchCurrentUser,
  useFetchUserImages,
} from "@/pages/browse/usersActions";
import { usePayload } from "@/utils/payloadProvider";

interface SidebarButtonProps {
  isMobile: boolean;
  route: string;
  icon: React.ReactNode;
  label: string;
  disabled?: boolean;
  onClick: () => void;
}

const getButtonStyles = (isActive: boolean, isMobile?: boolean) => ({
  backgroundColor: isActive ? theme.palette.action.selected : "transparent",
  borderRadius: isActive ? "12px" : "0", // Keep rounded corners on hover
  transform: isActive ? "scale(1.2)" : "", // Grow on hover
  paddingRight: "1rem",
  margin: isMobile ? 0 : "0.3rem 0",
  "&:hover": {
    backgroundColor: isActive
      ? theme.palette.action.selected
      : theme.palette.action.hover,
  },
});

const SidebarButton = ({
  isMobile,
  route,
  icon,
  label,
  disabled,
  onClick,
}: SidebarButtonProps) => {
  const location = useLocation();
  const isActive = location.pathname === route;

  return (
    <ListItemButton
      disableRipple
      disableTouchRipple
      onClick={onClick}
      disabled={disabled}
      sx={getButtonStyles(isActive, isMobile)}
    >
      <ListItemIcon>{icon}</ListItemIcon>
      {!isMobile && <ListItemText>{label}</ListItemText>}
    </ListItemButton>
  );
};

export const Sidebar = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
  const navigate = useNavigate();
  const logout = useLogout();
  const { data: userData } = useFetchCurrentUser();
  const { notifications, chats } = usePayload();
  const { data: userImages } = useFetchUserImages(userData?.id);

  const handleLogout = () => {
    logout();
  };

  const sidebarItems = [
    {
      route: routes.BROWSE,
      icon: <SearchIcon fontSize="small" />,
      label: "Browse",
      disabled: userData && (!userData.dateOfBirth || userImages.length === 0),
    },
    {
      route: routes.MATCHES,
      icon: <FavoriteIcon fontSize="small" />,
      label: "Matchas",
    },
    {
      route: routes.INSIGHTS,
      icon: <InsightsIcon fontSize="small" />,
      label: "Insights",
    },
    {
      route: routes.ME,
      icon: <AccountCircleIcon fontSize="small" />,
      label: "Profile",
    },
    {
      route: routes.CHAT,
      icon: (
        <>
          <Badge
            badgeContent={
              chats.filter((c) =>
                c.messages.some(
                  (m) => !m.isRead && m.fromUserId !== userData?.id
                )
              ).length
            }
            color="secondary"
            anchorOrigin={{ vertical: "top", horizontal: "right" }}
            aria-label="chat"
            max={99}
          >
            <ChatIcon fontSize="small" />
          </Badge>
        </>
      ),
      label: "Chat",
    },
    {
      route: routes.NOTIFICATIONS,
      icon: (
        <>
          <Badge
            badgeContent={
              notifications.filter((n) => n.isRead === false).length
            }
            color="secondary"
            anchorOrigin={{ vertical: "top", horizontal: "right" }}
            aria-label="notifications"
            max={99}
          >
            <NotificationsIcon fontSize="small" />
          </Badge>
        </>
      ),
      label: "Notifications",
    },
  ];

  return (
    <MatchaNavBar
      sx={{
        display: "flex",
        ...(isMobile
          ? {
              // Mobile styles
              flexDirection: "row",
              position: "fixed",
              zIndex: 10,
              bottom: 0,
              width: "100%",
              padding: 0,
              overflowX: "auto", // Enable horizontal scrolling
              overflowY: "hidden", // Prevent vertical scrolling
              whiteSpace: "nowrap", // Prevent content wrapping
              WebkitOverflowScrolling: "touch", // Smooth scrolling on iOS
              scrollbarWidth: "none", // Hide scrollbar in Firefox
              "&::-webkit-scrollbar": {
                // Hide scrollbar in Chrome/Safari
                display: "none",
              },
            }
          : {
              // Desktop styles
              position: "fixed",
              flexDirection: "column",
              minHeight: "100%",
              right: 0,
              padding: 0,
              flexGrow: 1,
              whiteSpace: "nowrap",
            }),
      }}
    >
      <Box
        sx={{
          display: "flex",
          flexDirection: isMobile ? "row" : "column",
          width: "100%",
        }}
      >
        {sidebarItems.map((item) => (
          <SidebarButton
            isMobile={isMobile}
            key={item.route}
            route={item.route}
            icon={item.icon}
            label={item.label}
            disabled={item.disabled}
            onClick={() => navigate(item.route)}
          />
        ))}
      </Box>
      <Box
        sx={{
          mt: isMobile ? 0 : "auto",
          width: isMobile ? "auto" : "100%",
        }}
      >
        {!isMobile && <Divider />}
        {/* Logout */}
        <ListItemButton
          disableRipple
          disableTouchRipple
          onClick={handleLogout}
          sx={getButtonStyles(false, isMobile)}
        >
          <ListItemIcon>
            <LogoutIcon fontSize="small" />
          </ListItemIcon>
          {!isMobile && <ListItemText>Logout</ListItemText>}
        </ListItemButton>
      </Box>
    </MatchaNavBar>
  );
};
