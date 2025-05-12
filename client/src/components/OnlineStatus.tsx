import { User } from "@/app/interfaces";
import { client } from "@/utils/axios";
import { useState, useEffect } from "react";

export const OnlineStatus = ({ user }: { user: Partial<User> }) => {
  const [online, setOnline] = useState(false);
  const [lastSeenText, setLastSeenText] = useState("Not available");
  const [loading, setLoading] = useState(true); // Add loading state

  // Retrieve online status
  const getOnlineStatus = async (): Promise<boolean> => {
    try {
      const response = await client.get(`/users/${user.id}/online/`);
      if (response.status === 200) {
        const data = response.data;

        // Check for successful response and expected data structure
        if (
          data &&
          data.success === true &&
          data.data &&
          typeof data.data.online === "boolean"
        ) {
          const isOnline = data.data.online;
          setOnline(isOnline); // Set online status state based on API response
          return isOnline; // Return the actual boolean online status
        } else {
          // Handle cases where success is false or data structure is unexpected
          console.warn(
            "API request successful but data indicates failure or unexpected structure:",
            data
          );
          setOnline(false); // Assume offline if data is not as expected or success is false
          return false; // Return false in these cases
        }
      }
    } catch (error) {
      console.error("Error fetching online status:", error);
      setOnline(false);
    }
    return false; // Return false if error or not 200
  };

  useEffect(() => {
    const checkOnlineStatus = async () => {
      setLoading(true); // Start loading
      setOnline(false); // Reset states
      setLastSeenText("Not available");

      if (user?.lastSeen && user?.id) {
        const lastSeenDate = new Date(user.lastSeen);

        if (!isNaN(lastSeenDate.getTime())) {
          const now = Date.now();
          const oneWeekAgo = now - 7 * 24 * 60 * 60 * 1000;

          const formattedLastSeen = lastSeenDate.toLocaleString("en-US", {
            month: "2-digit",
            day: "2-digit",
            year: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
          });

          if (lastSeenDate.getTime() > oneWeekAgo) {
            const isOnline = await getOnlineStatus(); // Wait for the result
            if (isOnline) {
              setLastSeenText("Online");
            } else {
              setLastSeenText(`Last seen: ${formattedLastSeen}`);
            }
          } else {
            setOnline(false); // Ensure online is false if last seen > 1 week ago
            setLastSeenText(formattedLastSeen);
          }
        } else {
          setLastSeenText("Invalid date");
        }
      } else {
        // Handle cases where user, lastSeen, or id is missing
        setLastSeenText("Not available");
        setOnline(false);
      }
      setLoading(false); // End loading
    };

    checkOnlineStatus();
  }, [user]); // Keep dependency array as is

  if (loading) {
    return (
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          backgroundColor: "#a9a9a9", // Use a neutral color for loading
          borderRadius: "5px",
          padding: "5px",
          width: "fit-content",
          margin: "0 auto",
          color: "white",
          fontSize: "12px",
          fontWeight: "bold",
          textTransform: "uppercase",
        }}
      >
        <span style={{ color: "white", fontSize: "10px" }}>Loading...</span>
      </div>
    );
  }

  return (
    <>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          backgroundColor: online ? "#4caf50" : "#a9a9a9",
          borderRadius: "5px",
          padding: "5px",
          width: "fit-content",
          margin: "0 auto",
          color: "white",
          fontSize: "12px",
          fontWeight: "bold",
          textTransform: "uppercase",
        }}
      >
        {/* Display logic remains similar, but relies on updated state */}
        {online ? (
          <span style={{ color: "white", fontSize: "10px" }}>Online</span>
        ) : (
          <span style={{ color: "white", fontSize: "10px" }}>
            {lastSeenText.startsWith("Last seen:")
              ? lastSeenText
              : `Last seen: ${lastSeenText}`}
          </span>
        )}
      </div>
    </>
  );
};
