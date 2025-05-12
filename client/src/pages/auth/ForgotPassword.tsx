import { useState } from "react";
import {
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  Link,
} from "@mui/material";
import { routes } from "@/utils/routes";
import { AuthLayout } from "@/components/AuthLayout";
import { useResetPassword } from "../browse/usersActions";

export const ForgotPassword = () => {
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [status, setStatus] = useState<"idle" | "success" | "error">("idle");
  const [message, setMessage] = useState("");
    const resetPassword = useResetPassword();
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    console.log("handleSubmit");
    resetPassword(email);
    setIsSubmitting(true);
    setStatus("idle");
    setMessage("");
  };

  return (
    <AuthLayout>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Enter your email address and we'll send you a link to reset your
        password
      </Typography>

      {status === "success" && (
        <Alert severity="success" sx={{ mb: 3 }}>
          {message}
        </Alert>
      )}

      {status === "error" && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {message}
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit} sx={{ width: "100%" }}>
        <TextField
          margin="normal"
          required
          fullWidth
          id="email"
          label="Email Address"
          name="email"
          autoComplete="email"
          autoFocus
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={isSubmitting || status === "success"}
        />

        <Button
          type="submit"
          fullWidth
          variant="contained"
          color="primary"
          sx={{ mt: 3, mb: 2 }}
          disabled={isSubmitting || status === "success"}
        >
          {isSubmitting ? (
            <CircularProgress size={24} color="inherit" />
          ) : (
            "Send Reset Link"
          )}
        </Button>
      </Box>

      <Box sx={{ mt: 2, textAlign: "center" }}>
        <Link href={routes.LOGIN} color="primary" underline="hover">
          Back to Login
        </Link>
      </Box>
    </AuthLayout>
  );
};
