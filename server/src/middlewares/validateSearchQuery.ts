import { z } from "zod";
import { Request, Response, NextFunction } from "express";
import { userSearchQuerySchema } from "@interfaces/userSearchQuery";

export const validateUserSearchQuery = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    const parsedData = userSearchQuerySchema.parse(req.query);
    req.body = parsedData;
    next();
  } catch (error) {
    if (error instanceof z.ZodError) {
      res.status(400).json({ error: error.errors });
    } else {
      res.status(400).json({ error: "Unknown error" });
    }
  }
};
