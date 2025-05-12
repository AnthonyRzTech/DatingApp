import { client } from "@/utils/axios";
import ReportIcon from "@mui/icons-material/Report";
import {
  Button,
  Snackbar,
  Alert,
  AlertTitle,
  Box,
  Button as MuiButton,
} from "@mui/material";
import { useState } from "react";

export const ReportButton = ({ userId }: { userId: number }) => {
  const [loading, setLoading] = useState(false);
  const [confirmationOpen, setConfirmationOpen] = useState(false);
  const [resultOpen, setResultOpen] = useState(false);
  const [resultMessage, setResultMessage] = useState("");
  const [resultSeverity, setResultSeverity] = useState<"success" | "error">(
    "success"
  );

  const handleReportClick = () => {
    setConfirmationOpen(true);
  };

  const handleConfirmReport = async () => {
    setConfirmationOpen(false);
    setLoading(true);

    try {
      const response = await client.post(`/users/${userId}/reports`);
      if (response && (response.status === 201 || response.status === 200)) {
        setResultMessage("User has been reported successfully.");
        setResultSeverity("success");
      } else {
        setResultMessage("Failed to report user. Please try again.");
        setResultSeverity("error");
      }
    } catch (error) {
      console.error("Error reporting user:", error);
      setResultMessage("Error reporting user. Please try again.");
      setResultSeverity("error");
    } finally {
      setLoading(false);
      setResultOpen(true);
    }
  };

  const handleCancelReport = () => {
    setConfirmationOpen(false);
  };

  return (
    <>
      <Button
        variant="outlined"
        color="warning"
        startIcon={<ReportIcon />}
        onClick={handleReportClick}
        size="small"
        sx={{ mt: 1 }}
        disabled={loading}
      >
        {loading ? "Reporting..." : "Report Fake Account"}
      </Button>

      <Snackbar
        open={confirmationOpen}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
        onClose={handleCancelReport}
      >
        <Alert severity="warning" variant="filled" sx={{ width: "100%" }}>
          <AlertTitle>Confirm Report</AlertTitle>
          Are you sure you want to report this user?
          <Box
            sx={{ mt: 2, display: "flex", justifyContent: "flex-end", gap: 1 }}
          >
            <MuiButton
              size="small"
              color="inherit"
              onClick={handleCancelReport}
              variant="outlined"
            >
              No
            </MuiButton>
            <MuiButton
              size="small"
              color="inherit"
              onClick={handleConfirmReport}
              variant="outlined"
            >
              Yes
            </MuiButton>
          </Box>
        </Alert>
      </Snackbar>

      <Snackbar
        open={resultOpen}
        autoHideDuration={6000}
        onClose={() => setResultOpen(false)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          onClose={() => setResultOpen(false)}
          severity={resultSeverity}
          variant="filled"
          sx={{ width: "100%" }}
        >
          {resultMessage}
        </Alert>
      </Snackbar>
    </>
  );
};
