import { useEffect } from "react";
import { Box, Paper } from "@mui/material";
import { useSearchParams } from "react-router-dom";
import { client } from "@utils/axios";
import { routes } from "@/utils/routes";
import { theme } from "@/components/theme";

export const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");

  useEffect(() => {
    const verifyEmail = async () => {
      try {
        const response = await client.get(`/auth/verify-email?token=${token}`);
      } catch (error: any) {
        console.error("Verification failed:", error);
      }
    };
    verifyEmail();
  }, [token]);

  return (
    <Box
      sx={{
        height: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        backgroundColor: theme.palette.background.default,
      }}
    >
      <Paper
        elevation={3}
        sx={{
          p: 4,
          maxWidth: 500,
          width: "90%",
          textAlign: "center",
          borderRadius: 2,
        }}
      >
        Email verified
      </Paper>
    </Box>
  );
};
