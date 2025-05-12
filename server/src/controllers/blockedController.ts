import { NextFunction, Request, Response } from "express";
import { blocked as blockedUserModel } from "@models/blockedModel";
import {
  handleBadRequestResponse,
  handleErrorResponse,
} from "@utils/errorHandler";

export const blockUser = async (req: Request, res: Response): Promise<void> => {
  try {
    const userId = req?.user?.id;
    const blockedUserId = parseInt(req.body.blockedUserId);
    if (userId === blockedUserId) {
      handleBadRequestResponse(res, "You cannot block yourself");
      return;
    }
    const blockedUser = await blockedUserModel.blockUser(
      userId!,
      blockedUserId
    );
    res.status(201).json({
      status: 201,
      data: blockedUser,
    });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const unblockUser = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    const blockedUserId = parseInt(req.body.blockedUserId);
    const blockedUser = await blockedUserModel.unblockUser(
      userId!,
      blockedUserId
    );
    res.status(200).json({
      status: 200,
      data: blockedUser,
    });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const getUserBlockedUsers = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    const blockedUsers = await blockedUserModel.getBlockedUsers(userId!);
    res.status(200).json({
      status: 200,
      data: blockedUsers,
    });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};
