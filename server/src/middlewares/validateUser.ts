import { z } from "zod";
import { Request, Response, NextFunction } from "express";
import {
  userCreationSchema,
  userLoginSchema,
  userUpdateSchema,
} from "@interfaces/userValidationSchema";
import { blockedValidationSchema } from "@interfaces/blockedValidationSchema";

export const validateUserCreation = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    userCreationSchema.parse(req.body);
    next();
  } catch (error) {
    if (error instanceof z.ZodError) {
      res.status(400).json({ error: error.errors });
    } else {
      res.status(400).json({ error: "Unknown error" });
    }
  }
};

export const validateUserLogin = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    userLoginSchema.parse(req.body);
    next();
  } catch (error) {
    if (error instanceof z.ZodError) {
      res.status(400).json({ error: error.errors });
    } else {
      res.status(400).json({ error: "Unknown error" });
    }
  }
};

export const validateUserUpdate = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    userUpdateSchema.parse(req.body);
    next();
  } catch (error) {
    if (error instanceof z.ZodError) {
      res.status(400).json({ error: error.errors });
    } else {
      res.status(400).json({ error: "Unknown error" });
    }
  }
};

export const validateUserBlocked = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    blockedValidationSchema.parse({userId: req?.user?.id, blockeUserId: req.body.blockedUserId});
    next();
  } catch (error) {
    if (error instanceof z.ZodError) {
      res.status(400).json({ error: error.errors });
    } else {
      res.status(400).json({ error: "Unknown error" });
    }
  }
}   

//validation user params for advanced search
