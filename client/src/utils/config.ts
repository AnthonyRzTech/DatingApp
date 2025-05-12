import { UserSearchQuery } from "@app/interfaces";

/* ___________________________________ Server ___________________________________ */

// https://create-react-app.dev/docs/adding-custom-environment-variables/

export const SERVER_URL =
  process.env.REACT_APP_API_URL || "http://localhost:8000";
export const SOCKET_URL =
  process.env.REACT_APP_SOCKET_URL || "http://localhost:8000";

/* ________________________________ Query Params ________________________________ */

export const MIN_AGE = 18;
export const MAX_AGE = 90;

export const MIN_FAME = 0;
export const MAX_FAME = 100;

export const RECOMMENDED_DISTANCE = 20000;
export const COORDINATE = {
  // 42 Paris
  latitude: 48.896683,
  longitude: 2.318388,
};

export const DEFAULT_SEARCH_PARAMS: UserSearchQuery = {
  ageMin: MIN_AGE,
  ageMax: MAX_AGE,
  minFameRating: MIN_FAME,
  maxFameRating: MAX_FAME,
  distance: RECOMMENDED_DISTANCE,
  latitude: COORDINATE.latitude,
  longitude: COORDINATE.longitude,
};
