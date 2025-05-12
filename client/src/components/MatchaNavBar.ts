import { styled } from "@mui/material/styles";
import { List, ListProps } from "@mui/material";

export const MatchaNavBar = styled(List)<ListProps>(({ theme }) => ({
  backgroundColor: theme.palette.primary.main,
  color: theme.palette.primary.contrastText,
  // height: "100vh",
  "& .MuiListItemIcon-root": {
    minWidth: "2em"
  },
  [theme.breakpoints.down('sm')]: {
    height: "auto",
    zIndex: 1100
  }
}));
