import { Request, Response, NextFunction } from "express";
import { JWT_SECRET_KEY } from "../settings";
import jwt from "jsonwebtoken";
import { Socket } from "socket.io";

// Interface for JWT payload
interface JWTPayload {
  id: number;
  // Add other payload fields you need
}

// Extend Express Request to include user
declare global {
  namespace Express {
    interface Request {
      user?: JWTPayload;
    }
  }
}

export const authenticateToken = async (
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> => {
  const authToken = req.cookies?.authToken;

  if (!authToken) {
    res.status(401).json({ message: "No auth token found" });
    return;
  }

  try {
    const decoded = jwt.verify(
      authToken,
      JWT_SECRET_KEY as string
    ) as unknown as JWTPayload;

    if (!decoded.id) {
      res.status(403).json({ message: "Invalid token payload" });
      return;
    }

    req.user = decoded;
    next();
  } catch (err) {
    if (err instanceof jwt.TokenExpiredError) {
      res.status(403).json({ message: "Token expired" });
      return;
    }
    console.error(err);
    res.status(403).json({ message: "Invalid token" });
    return;
  }
};

// WebSocket authentication middleware
export const authenticateSocketToken = (
  req: Request,
  res: Response,
  next: NextFunction,
  socket: Socket
): void => {
  const authToken = req.cookies?.authToken;

  if (!authToken) {
    return next(new Error("No auth token found"));
  }

  try {
    const decoded = jwt.verify(
      authToken,
      JWT_SECRET_KEY as string
    ) as JWTPayload;

    if (!decoded.id) {
      return next(new Error("Invalid token payload"));
    }

    return next();
  } catch (err) {
    console.error("Socket authentication error:", err);
    return next(new Error("Invalid token"));
  }
};
