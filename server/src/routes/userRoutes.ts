import express, { Router } from "express";
import * as UserSearchController from "@controllers/userSearchController";
import * as UserController from "@controllers/userController";
import * as TagViews from "@controllers/tagController";
import * as LikeViews from "@controllers/likeController";
import * as MatchViews from "@controllers/matchController";
import * as ImageViews from "@controllers/imageController";
import * as ViewsController from "@controllers/viewController";
import {
  getUserBlockedUsers,
  blockUser,
  unblockUser,
} from "@controllers/blockedController";
import {
  validateUserUpdate,
  validateUserBlocked,
} from "../middlewares/validateUser";
import { validateUserSearchQuery } from "@middlewares/validateSearchQuery";
import { getUserReport, reportUser } from "@controllers/reportsController";

const router: Router = express.Router();

// Search routes
router.get(
  "/search",
  validateUserSearchQuery,
  UserSearchController.searchUsers
);

router.get("/me", UserController.getCurrentUser);
router.get("/:id", UserController.getUserById);
router.put("/:id", validateUserUpdate, UserController.updateUser);

router.get("/:id/online", UserController.getUserOnlineStatus);

router.get("/:id/reports", getUserReport);
router.post("/:id/reports", reportUser);

router.get("/:id/blocked", validateUserBlocked, getUserBlockedUsers);
router.post("/:id/blocked", validateUserBlocked, blockUser);
router.delete("/:id/blocked", validateUserBlocked, unblockUser);

router.get("/:id/tags", TagViews.getUserTags);
router.post("/:id/tags", TagViews.addUserTag);
router.delete("/:id/tags", TagViews.deleteUserTag);

router.get("/:id/liked", LikeViews.checkUserLiked);
router.get("/:id/likes", LikeViews.getUserLikes);
router.post("/:id/likes", LikeViews.addUserLike);
router.delete("/:id/likes", LikeViews.deleteUserLike);

router.get("/:id/views", ViewsController.getUserViews);
router.post("/:id/views", ViewsController.addUserView);

router.get("/:id/matches", MatchViews.getUserMatches);
router.delete("/:id/matches", MatchViews.deleteUserMatch);

router.get("/:id/images", ImageViews.getUserImages);
router.post("/:id/images", ImageViews.createUserImage);
router.delete("/:id/images", ImageViews.deleteUserImage);
router.put("/:id/images", ImageViews.updateUserImageStatus);
router.get("/:id/images/profile", ImageViews.getUserProfileImage);

export default router;
