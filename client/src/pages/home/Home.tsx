import { Stack, Button, Typography, Container } from "@mui/material";
import { Navigate, useNavigate } from "react-router-dom";
import { routes } from "@utils/routes";
import { useAuth } from "@utils/authContext";
import LoadingCup from "@/components/LoadingCup/LoadingCup";
import { WaveForm } from "@/components/Pattern/waveForm";

export const Home = () => {
  const navigate = useNavigate();
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <LoadingCup />;

  if (isAuthenticated) {
    return <Navigate to={routes.BROWSE} replace />;
  }

  return (
    <>
      <WaveForm>
        <Container
          maxWidth="sm"
          sx={{
            height: "100vh",
            display: "flex",
            flexDirection: "column",
            justifyContent: "center",
            alignItems: "center",
            gap: 4,
            zIndex: 1,
          }}
        >
          <Typography
            variant="h1"
            component="h1"
            fontSize={84}
            textAlign="center"
            fontFamily="Nunito, sans-serif"
          >
            Welcome to Matcha
          </Typography>
          <Stack spacing={2} direction="row">
            <Button variant="contained" onClick={() => navigate(routes.LOGIN)}>
              Log in
            </Button>
            <Button
              variant="contained"
              onClick={() => navigate(routes.REGISTER)}
            >
              Sign up
            </Button>
          </Stack>
        </Container>
      </WaveForm>
    </>
  );
};
