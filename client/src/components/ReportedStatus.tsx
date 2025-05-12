import WarningAmberIcon from "@mui/icons-material/WarningAmber";
import Tooltip from "@mui/material/Tooltip";
import { User } from "@/app/interfaces";
import { useEffect, useState } from "react";
import { client } from "@/utils/axios";

export const ReportedIndicator = ({ user }: { user: Partial<User> }) => {
  const [isReportedFake, setIsReportedFake] = useState<boolean>(false);

  const fetchReportedStatus = async () => {
    try {
      const response = await client.get(`/users/${user.id}/reports`);
      if (response.status === 200) {
        // Check for successful response and expected data structure
        const data = response.data;
        if (data && data.success === true && data.data) {
          const reportCount = data.data.report_count;
          setIsReportedFake(reportCount > 0);
        } else {
          // Handle cases where success is false or data structure is unexpected
          console.warn(
            "API request successful but data indicates failure or unexpected structure:",
            data
          );
          setIsReportedFake(false); // Assume not reported if data is not as expected or success is false
        }
      }
    } catch (error) {
      console.error("Error fetching reported status:", error);
    }
  };

  useEffect(() => {
    const checkReportedStatus = async () => {
      setIsReportedFake(false); // Reset state before fetching
      await fetchReportedStatus();
    };

    if (user?.id) {
      checkReportedStatus();
    }
  }, [user?.id]);

  return (
    <>
      {isReportedFake && (
        <Tooltip title="This account has been reported as potentially fake.">
          <WarningAmberIcon color="warning" />
        </Tooltip>
      )}
    </>
  );
};
