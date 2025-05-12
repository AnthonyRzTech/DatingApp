import { User, UserBlock } from "@/app/interfaces";

export function formatCoordinates(obj: any) {
  if (
    typeof obj !== "object" ||
    obj === null ||
    !("x" in obj) ||
    !("y" in obj)
  ) {
    return "";
  }
  const { x, y } = obj;
  return `(${x}, ${y})`;
}

export const isValidEmail = (email: string): boolean => {
  const emailRegex =
    /^([A-Z0-9_+-]+\.?)*[A-Z0-9_+-]@([A-Z0-9][A-Z0-9-]*\.)+[A-Z]{2,}$/i;
  return emailRegex.test(email);
};

export const isValidUsername = (username: string): boolean => {
  const usernameRegex = /^[a-zA-Z0-9_]*$/;
  return usernameRegex.test(username);
};

export async function getIpData() {
  try {
    const response = await fetch("https://ipapi.co/json/");
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error fetching IP data:", error);
    throw error;
  }
}

export const capitalize = (str?: string): string => {
  if (!str) return "";
  return str.charAt(0).toUpperCase() + str.slice(1);
};

export const formatPreferences = (preferences: string): string[] => {
  if (!preferences) return [];
  return preferences
    .slice(1)
    .slice(0, -1)
    .split(",")
    .map((preference) => preference.trim());
};

export const filterBlockedUsers = (
  users?: User[],
  blockedUsers?: UserBlock[]
) => {
  if (!users) return [];
  if (!blockedUsers) return users;
  return users.filter(
    (user) =>
      !blockedUsers.some(
        (blocked: UserBlock) => blocked.blockedUserId === user.id
      )
  );
};

export const calculateAge = (birthDate: Date): number => {
  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDifference = today.getMonth() - birthDate.getMonth();
  if (
    monthDifference < 0 ||
    (monthDifference === 0 && today.getDate() < birthDate.getDate())
  ) {
    age--;
  }
  return age;
};
