import { SOCKET_EVENTS } from "@src/interfaces/socketEventsInterface";

export interface NotificationPayload {
  id: number;
  type: NotificationType;
  ui_variant?: "default" | "success" | "info" | "warning" | "error";
  message: string;
  fromUserId: number;
  toUserId: number;
  isRead: boolean;
  createAt: Date;
}

export enum NotificationType {
  LIKE = SOCKET_EVENTS.LIKE,
  UNLIKE = SOCKET_EVENTS.UNLIKE,
  MATCH = SOCKET_EVENTS.MATCH,
  MESSAGE = SOCKET_EVENTS.MESSAGE,
  VIEW = SOCKET_EVENTS.VIEW,
}

export interface NotificationContent {
  message: string;
  // Possibly add more fields here
}

export interface NotificationInterface {
  id?: number;
  type: NotificationType;
  ui_variant?: "default" | "success" | "info" | "warning" | "error";
  content: NotificationContent;
  toUserID: number;
  fromUserID: number;
  isRead?: boolean;
  status?: NotificationStatus;
  readAt?: Date;
  createdAt?: Date;
}

export enum NotificationStatus {
  PENDING = "PENDING",
  SENT = "SENT",
  FAILED = "FAILED",
}
