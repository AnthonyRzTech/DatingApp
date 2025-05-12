import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import {
  Box,
  TextField,
  Button,
  Typography,
  CircularProgress,
} from "@mui/material";
import { useSnackbar } from "notistack";
import { AuthLayout } from "@/components/AuthLayout";
import { theme } from "@/components/theme";
import { useUpdatePassword } from "../browse/usersActions";

export const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");
  const { enqueueSnackbar } = useSnackbar();
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const updatePassword = useUpdatePassword();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      enqueueSnackbar("Passwords do not match", { variant: "error" });
      return;
    }

    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

    if (!passwordRegex.test(password)) {
      enqueueSnackbar(
        "Password must be at least 8 characters long, include an uppercase letter, a lowercase letter, a number, and a special character.",
        { variant: "error" }
      );
      return;
    }

    setIsSubmitting(true);
     updatePassword({
        password,
        token: token || "",
      });
  };

  return (
    <AuthLayout>
      <Typography
        component="h1"
        variant="h4"
        sx={{
          mb: theme.spacing(3),
          color: "primary.main",
          fontWeight: "bold",
        }}
      >
        Reset Password
      </Typography>

      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{ mt: 1, width: "100%" }}
      >
        <TextField
          margin="normal"
          required
          fullWidth
          id="password"
          label="New Password"
          type="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={isSubmitting}
          sx={{ mb: 2 }}
        />

        <TextField
          margin="normal"
          required
          fullWidth
          id="confirmPassword"
          label="Confirm New Password"
          type="password"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={isSubmitting}
          sx={{ mb: 3 }}
        />

        <Button
          type="submit"
          fullWidth
          variant="contained"
          color="primary"
          disabled={isSubmitting}
          sx={{ mt: 1, mb: 3, py: 1.5 }}
        >
          {isSubmitting ? (
            <CircularProgress size={24} color="inherit" />
          ) : (
            "Reset Password"
          )}
        </Button>
      </Box>
    </AuthLayout>
  );
};
