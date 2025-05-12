import { Request, Response } from "express";
import {
  handleErrorResponse,
  handleBadRequestResponse,
  handleForbiddenResponse,
  handleNotFoundResponse,
} from "@utils/errorHandler";
import { likeModel } from "@models/likeModel";
import { matchModel } from "@models/matchModel";
import {
  NotificationInterface,
  NotificationPayload,
  NotificationStatus,
  NotificationType,
} from "@interfaces/notificationInterface";
import { addNotification, addChatEvent } from "./notificationController";
import { user } from "@src/models/userModel";
import { chatModel } from "@src/models/chatModel";
import { updateFameRate } from "./userController";

export const checkUserLiked = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const likerUserId = req?.user?.id;
    const likedUserId = parseInt(req.params.id);
    if (!likedUserId) {
      handleBadRequestResponse(res, "likedUserId is required");
      return;
    }
    if (likerUserId === likedUserId) {
      handleBadRequestResponse(res, "You cannot like yourself");
      return;
    }
    const response = await likeModel.checkUserLiked(likerUserId!, likedUserId);
    if (response.length > 0) {
      res.status(200).json({ status: 200, data: { isLiked: true } });
    } else {
      res.status(200).json({ status: 200, data: { isLiked: false } });
    }
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const getUserLikes = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = parseInt(req.params.id);
    if (!userId) {
      handleBadRequestResponse(res, "userId is required");
      return;
    }
    if (req?.user?.id !== userId) {
      handleForbiddenResponse(
        res,
        "You are not authorized to view this user's likes"
      );
      return;
    }

    const likes = await likeModel.getUserLikes(userId!);
    res.status(200).json({ status: 200, data: likes });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const addUserLike = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const likerUserId = req?.user?.id;
    const likedUserId = parseInt(req.params.id);
    if (!likedUserId || !likerUserId) {
      handleBadRequestResponse(res, "likedUserId is required");
      return;
    }
    if (likerUserId === likedUserId) {
      handleBadRequestResponse(res, "You cannot like yourself");
      return;
    }

    const like = await likeModel.addUserLike(likerUserId, likedUserId);
    const match = await matchModel.checkForMatch(likerUserId, likedUserId);
    updateFameRate(likedUserId);
    let notification: NotificationInterface;
    if (match) {
      notification = {
        type: NotificationType.MATCH,
        ui_variant: "success",
        content: { message: "You have a new match!" },
        toUserID: likedUserId,
        fromUserID: likerUserId,
      };
      await addNotification(notification, [likedUserId, likerUserId]);
      // TODO: make an event to update the chats list for both users
      const res = await chatModel.createChat(likedUserId, likerUserId);
      if (res) {
        await addChatEvent(res, [likedUserId, likerUserId]);
      }
    } else if (like.length === 1) {
      notification = {
        type: NotificationType.LIKE,
        ui_variant: "info",
        content: { message: "You have a new like!" },
        toUserID: likedUserId,
        fromUserID: likerUserId,
      };
      await addNotification(notification, [likedUserId]);
    }

    res.status(201).json({ status: 201, message: "Like added" });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const deleteUserLike = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const likerUserId = req?.user?.id;
    const likedUserId = parseInt(req.params.id);
    if (!likedUserId) {
      handleBadRequestResponse(res, "likedUserId is required");
      return;
    }

    await likeModel.deleteUserLike(likerUserId!, likedUserId);
    updateFameRate(likedUserId);
    res.status(200).json({ status: 200, message: "Like deleted" });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};
