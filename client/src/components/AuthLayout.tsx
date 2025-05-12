import { Container, Paper } from "@mui/material";
import { theme } from "@components/theme";
import { Props } from "@app/interfaces";

export const AuthLayout: React.FC<Props> = (props) => {
  return (
    <Container maxWidth="xs" sx={{ paddingTop: theme.spacing(8) }}>
      <Paper
        elevation={2}
        sx={{
          p: theme.spacing(4),
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          borderRadius: 2,
          bgcolor: "background.paper",
        }}
      >
        {props.children}
      </Paper>
    </Container>
  );
};
