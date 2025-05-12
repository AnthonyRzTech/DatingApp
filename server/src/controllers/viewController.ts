import { Request, Response } from "express";
import {
  handleErrorResponse,
  handleBadRequestResponse,
  handleForbiddenResponse,
  handleNotFoundResponse,
} from "@utils/errorHandler";
import { viewModel } from "@models/viewModel";
import {
  NotificationInterface,
  NotificationType,
} from "@src/interfaces/notificationInterface";
import { addNotification } from "./notificationController";
import { user } from "@src/models/userModel";

export const getUserViews = async (
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
        "You are not authorized to view this user's views"
      );
      return;
    }

    const views = await viewModel.getUserViews(userId!);
    res.status(200).json({ status: 200, data: views });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const addUserView = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const viewedUserId = parseInt(req.params.id);
    const userId = req?.user?.id;
    if (!viewedUserId) {
      handleBadRequestResponse(res, "userId is required");
      return;
    }
    if (userId === viewedUserId) {
      handleForbiddenResponse(
        res,
        "You are not authorized to view your own profile"
      );
      return;
    }

    const result = await viewModel.addUserView(userId!, viewedUserId!);
    const viewerUsername = await user.getUsernameById(userId!);
    if (result) {
      let notification: NotificationInterface = {
        type: NotificationType.VIEW,
        content: {
          message: `${viewerUsername} viewed your profile`,
        },
        fromUserID: userId!,
        toUserID: viewedUserId!,
        isRead: false,
        createdAt: new Date(),
      };
      await addNotification(notification, [viewedUserId!]);
    }
    res.status(201).json({ status: 201, message: "View added successfully" });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};
