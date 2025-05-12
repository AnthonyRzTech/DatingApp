import { z } from "zod";
import { Gender } from "./userInterface";

export const userCreationSchema = z
  .object({
    firstName: z
      .string({
        required_error: "First name is required",
      })
      .trim()
      .min(1, "First name must be at least 1 characters"),
    lastName: z
      .string({
        required_error: "Last name is required",
      })
      .trim()
      .min(1, "Last name must be at least 1 characters"),
    username: z
      .string({
        required_error: "Username is required",
      })
      .trim()
      .regex(
        /^[a-zA-Z0-9_]*$/,
        "Username can only contain letters, numbers, and underscores"
      )
      .min(3, "Username must be at least 3 characters"),
    email: z
      .string({
        required_error: "Email is required",
      })
      .trim()
      .email("Invalid email format"),
    password: z
      .string({
        required_error: "Password is required",
      })
      .trim()
      .min(8, "Password must be at least 8 characters")
      .max(32)
      .regex(
        /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$/,
        "Password must contain at least one letter, one number and one special character"
      ),
  })
  .strict();

export const userLoginSchema = z
  .object({
    username: z
      .string({
        required_error: "Username is required",
      })
      .trim()
      .min(3, "Username must be at least 3 characters"),
    password: z
      .string({
        required_error: "Password is required",
      })
      .trim()
      .min(8, "Password must be at least 8 characters")
      .max(32)
      .regex(
        /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$/,
        "Password must contain at least one letter, one number and one special character"
      ),
  })
  .strict();

export const userUpdateSchema = z
  .object({
    firstName: z
      .string()
      .trim()
      .min(1, "First name must be at least 1 characters"),
    lastName: z
      .string()
      .trim()
      .min(1, "Last name must be at least 1 characters"),
    username: z
      .string()
      .trim()
      .regex(
        /^[a-zA-Z0-9_]*$/,
        "Username can only contain letters, numbers, and underscores"
      )
      .min(3, "Username must be at least 3 characters"),
    email: z.string().trim().email("Invalid email format"),
    gender: z.enum(["male", "female", "other"]),
    preferences: z.array(z.enum([Gender.Male, Gender.Female, Gender.Other])),
    dateOfBirth: z.date(),
    bio: z.string().max(500, "Bio must be at most 500 characters"),
    location: z
      .string()
      .regex(
        /^\(\s*-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?\s*\)$/,
        "Location must be in format '(x,y)'"
      )
      .transform((val) => {
        const [x, y] = val
          .slice(1, -1)
          .split(",")
          .map((num) => parseFloat(num.trim()));

        if (y < -90 || y > 90)
          throw new Error("Latitude must be between -90 and 90");
        if (x < -180 || x > 180)
          throw new Error("Longitude must be between -180 and 180");

        return { latitude: y, longitude: x };
      }),
    lastSeen: z.date(),
    updated: z.date(),
  })
  .partial();
