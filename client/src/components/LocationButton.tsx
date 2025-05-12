import { useState } from 'react';
import { getIpData } from "@/utils/helpers";
import { FieldProps } from "@rjsf/utils";
import { Button, CircularProgress } from "@mui/material";

export const LocationButton = (props: FieldProps) => {
  const { onChange } = props;
  const [loading, setLoading] = useState(false);

  const getLocationFromIP = async () => {
    try {
      const data = await getIpData();
      onChange({ x: data.latitude, y: data.longitude });
    } catch (e) {
      console.log("IP geolocation failed:", e);
    }
    setLoading(false);
  };

  const handleLocationUpdate = async () => {
    setLoading(true);
    
    if ("geolocation" in navigator) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          onChange({
            x: position.coords.latitude,
            y: position.coords.longitude,
          });
          setLoading(false);
        },
      );
    } else {
      getLocationFromIP();
    }
  };

  return (
    <Button 
      variant="contained" 
      onClick={handleLocationUpdate}
      disabled={loading}
    >
      {loading ? <CircularProgress size={24} /> : "Locate Me!"}
    </Button>
  );
};
