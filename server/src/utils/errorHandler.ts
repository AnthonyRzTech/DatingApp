import { Response } from "express";

export const handleErrorResponse = (res: Response, error: unknown): void => {
  res.status(500).json({
    status: 500,
    message: (error as Error).message,
  });
};

export const handleNotFoundResponse = (
  res: Response,
  message: string
): void => {
  res.status(404).json({
    status: 404,
    message,
  });
};

export const handleForbiddenResponse = (
  res: Response,
  message: string
): void => {
  res.status(403).json({
    status: 403,
    message,
  });
};

export const handleBadRequestResponse = (
  res: Response,
  message: string
): void => {
  res.status(400).json({
    status: 400,
    message,
  });
};
