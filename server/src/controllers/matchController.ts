import { Request, Response } from "express";
import {
  handleErrorResponse,
  handleBadRequestResponse,
  handleForbiddenResponse,
  handleNotFoundResponse,
} from "@utils/errorHandler";
import { matchModel } from "@models/matchModel";
import {
  NotificationInterface,
  NotificationType,
} from "@src/interfaces/notificationInterface";
import { user } from "@src/models/userModel";
import { addNotification, deleteChatEvent } from "./notificationController";
import { likeModel } from "@src/models/likeModel";
import { chatModel } from "@src/models/chatModel";

export const getUserMatches = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    if (!userId) {
      handleForbiddenResponse(
        res,
        "You must be logged in to view your matches"
      );
      return;
    }
    const matches = await matchModel.getUserMatches(userId!);
    res.status(200).json({ status: 200, data: matches });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const deleteUserMatch = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    if (!userId) {
      handleForbiddenResponse(res, "You must be logged in to delete a match");
      return;
    }
    const matchedUserId = parseInt(req.params.id);
    if (!matchedUserId) {
      handleBadRequestResponse(res, "Valid matched user ID is required");
      return;
    }
    if (matchedUserId === userId) {
      handleBadRequestResponse(res, "You cannot delete a match with yourself");
      return;
    }

    const match = await matchModel.removeUserMatch(userId, matchedUserId);
    const remove_like = await likeModel.deleteUserLike(userId, matchedUserId);
    // Check if there is a match
    let notification: NotificationInterface;
    if (match) {
      const username: string =
        (await user.getUsernameById(userId)) || String(userId);
      notification = {
        type: NotificationType.UNLIKE,
        ui_variant: "warning",
        content: { message: `You have been unliked by ${username}` },
        toUserID: matchedUserId,
        fromUserID: userId,
      };
      await addNotification(notification, [matchedUserId]);
      const result = await chatModel.deleteChat(userId, matchedUserId);
      if (result && result.id) {
        deleteChatEvent(result.id, [userId, matchedUserId]);
      }
    }
    res
      .status(200)
      .json({ status: 200, message: "Match deleted successfully" });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};
