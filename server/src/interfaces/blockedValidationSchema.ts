import { z } from "zod";

export const blockedValidationSchema = z
    .object({
        userId: z.number({
        required_error: "User ID is required",
        }),
        blockedUserId: z.number({
        required_error: "Blocked user ID is required",
        }),
    })
    .partial();