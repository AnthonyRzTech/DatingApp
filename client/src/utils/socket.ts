import { io } from "socket.io-client";
import { SOCKET_URL } from "@/utils/config";

/* ______________________________ Socket Events ______________________________ */
export enum SOCKET_EVENTS {
  // System events
  CONNECT = "connect",
  CONNECT_ERROR = "connect_error",
  DISCONNECT = "disconnect",
  JOIN = "join",
  JOIN_ERROR = "joinError",
  ERROR = "error",
  // Notification events
  NOTIFICATION_NEW = "notificationNew",
  NOTIFICATIONS_FETCH = "notificationFetch",
  NOTIFICATIONS = "notifications",
  NOTIFICATIONS_CLEAR = "notificationsClear",
  NOTIFICATION_DELETE = "notificationDelete",
  NOTIFICATION_READ = "notificationRead",
  NOTIFICATION_UNREAD = "notificationUnread",
  // Chats events
  CHATS_FETCH = "chatsFetch",
  CHATS_NEW = "chatsNew",
  CHATS_DELETE = "chatsDelete",
  MESSAGE_NEW = "messageNew",
  MESSAGES_READ = "messageRead",
  MESSAGE = "message",
  // Matching events
  MATCH = "match",
  LIKE = "like",
  UNLIKE = "unlike", // If user are already matched
  VIEW = "view",
}

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
  id: number;
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

export interface Message {
  id: number;
  chatId: number;
  content: string;
  fromUserId: number;
  createdAt: Date;
  isRead?: boolean;
}

export interface ChatInterface {
  id: number;
  messages: Message[];
  user1Id: number;
  user2Id: number;
  createdAt: Date;
  deletedBy: number[];
}

/* ________________________________ Socket.io ________________________________ */

export const initializeSocket = (userId: number) => {
  console.log("Connecting to Socket.IO server:", SOCKET_URL);

  const socket = io(SOCKET_URL, {
    withCredentials: true,
    // Try explicitly setting path
    path: "/socket.io/",
    // Try without automatic reconnection for testing
    reconnection: false,
  });

  socket.on("connect_error", (error) => {
    console.error("Socket connection error:", error.message);
  });

  socket.on(SOCKET_EVENTS.CONNECT, () => {
    console.log("Socket: Connected to server");

    const userRoom = `user_${userId}`;
    socket.emit(SOCKET_EVENTS.JOIN, userRoom);
    socket.on(SOCKET_EVENTS.JOIN_ERROR, (error: string) => {
      console.log("Socket: Join Error", error);
      socket.disconnect();
    });
  });

  socket.on(SOCKET_EVENTS.DISCONNECT, () => {
    console.log("Socket: Disconnected from server");
  });

  return socket;
};
