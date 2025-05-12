import { Link, Typography, Box } from "@mui/material";
import { theme } from "@components/theme";
import { LoginForm } from "@/components/LoginForm";
import { routes } from "@/utils/routes";
import { AuthLayout } from "@/components/AuthLayout";

export const Login = () => {
  return (
    <AuthLayout>
      {/* <Typography
        component="h1"
        variant="h4"
        sx={{
          mb: theme.spacing(4),
          color: "primary.main",
          fontWeight: "bold",
        }}
      >
        Sign In
      </Typography> */}

      <LoginForm />

      <Box sx={{ mt: theme.spacing(3) }}>
        <Typography
          variant="body2"
          sx={{
            display: "flex",
            alignItems: "center",
            gap: 1,
          }}
        >
          Don't have an account?
          <Link href={routes.REGISTER} color="primary" underline="hover">
            Sign up
          </Link>
        </Typography>
        <Typography
          variant="body2"
          sx={{
            display: "flex",
            alignItems: "center",
            gap: 1,
          }}
        >
          Forgot password?
          <Link href={routes.FORGOT_PASSWORD} color="primary" underline="hover">
            Click here
          </Link>
        </Typography>
      </Box>
    </AuthLayout>
  );
};
