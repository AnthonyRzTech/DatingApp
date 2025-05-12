import { z } from "zod";

export const TagSchema = z.object({
  tagId: z.coerce.number().int().positive(),
});
