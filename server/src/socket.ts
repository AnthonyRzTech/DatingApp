import { Server, Socket } from "socket.io";
import { Server as HttpServer } from "http";
import jwt from "jsonwebtoken";
import cookieParser from "cookie-parser";
import { FRONTEND_ORIGIN } from "@settings";
import { SOCKET_EVENTS } from "@src/interfaces/socketEventsInterface";
import { authenticateSocketToken} from "./middlewares/auth";
import { notificationModel } from "@src/models/notificationModel";
import { chatModel } from "@src/models/chatModel";
import { Message } from "./interfaces/chatInterface";
import { addNotification } from "@controllers/notificationController";
import { NotificationType } from "./interfaces/notificationInterface";
import { user } from "./models/userModel";

export const initializeSocket = (httpServer: HttpServer) => {
  /* ________________________________ Socket Setup ________________________________ */
  const io = new Server(httpServer, {
    cors: {
      origin: FRONTEND_ORIGIN,
      credentials: true,
    },
  });

  // Add middleware to parse cookies
  io.engine.use(cookieParser());

  // Add middleware to authenticate token
  io.engine.use(authenticateSocketToken);

  // Log connection errors
  io.engine.on("connection_error", (err: Error) => {
    console.log("[Socket] Connection error:", err);
  });

  // Socket.IO connection handling
  io.on(SOCKET_EVENTS.CONNECT, (socket: Socket) => {
    console.log("[Socket] Client connected:", socket.id);

    /* ________________________________ Socket Data ________________________________ */

    // Get auth token from cookie
    const cookies = socket.handshake.headers.cookie?.toString();
    const authToken = cookies
      ?.split(";")
      .find((c) => c.trim().startsWith("authToken="))
      ?.split("=")[1];
    if (!authToken) {
      socket.emit(SOCKET_EVENTS.ERROR, { message: "No auth token found" });
      return;
    }
    // Add user id to socket data
    socket.data.user = (jwt.decode(authToken) as { id: number }).id;
    socket.data.userRoom = `user_${socket.data.user}`;

    // Listen for client joining their own room
    socket.on(SOCKET_EVENTS.JOIN, async (room: string) => {
      try {
        const userRoom = socket.data.userRoom;
        // Check basic room name format based on user id
        if (room !== userRoom) {
          socket.emit(SOCKET_EVENTS.ERROR, { message: "Invalid room name" });
          return;
        }

        // Check if user already in room
        const rooms = io.sockets.adapter.rooms;
        if (rooms.get(userRoom)) {
          socket.emit(SOCKET_EVENTS.JOIN_ERROR, { message: "Already in room" });
          return;
        }

        await socket.join(userRoom);
        socket.emit(SOCKET_EVENTS.MESSAGE, {
          message: "Welcome to the server - user : " + socket.data.user,
        });
      } catch (error) {
        console.error("[Socket] Socket join error:", error);
        socket.emit(SOCKET_EVENTS.ERROR, { message: "Failed to join room" });
      }
    });

    /* ________________________ Notification Events _______________________ */

    socket.on(SOCKET_EVENTS.NOTIFICATIONS_FETCH, async (callback) => {
      console.log(
        "[Socket] Fetching notifications for user:",
        socket.data.user
      );
      // Fetch notifications from database
      try {
        const notifications = await notificationModel.getNotifications(
          socket.data.user
        );
        console.log("[Socket] Notifications fetched");
        callback(notifications);
      } catch (error) {
        console.error("[Socket] Error fetching notifications:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to fetch notifications",
        });
      }
    });

    socket.on(SOCKET_EVENTS.NOTIFICATION_READ, async (id: number) => {
      console.log("[Socket] Marking notification as read:", id);
      // Mark notification as read in database
      try {
        await notificationModel.markNotificationAsRead([id]);
      } catch (error) {
        console.error("[Socket] Error marking notification as read:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to mark notification as read",
        });
      }
    });

    socket.on(SOCKET_EVENTS.NOTIFICATION_UNREAD, async (id: number) => {
      console.log("[Socket] Marking notification as unread:", id);
      // Mark notification as unread in database
      try {
        await notificationModel.markNotificationAsUnread([id]);
      } catch (error) {
        console.error("[Socket] Error marking notification as unread:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to mark notification as unread",
        });
      }
    });

    socket.on(SOCKET_EVENTS.NOTIFICATION_DELETE, async (id: number) => {
      console.log("[Socket] Deleting notification:", id);
      // Delete notification from database
      try {
        await notificationModel.deleteNotification(id);
      } catch (error) {
        console.error("[Socket] Error deleting notification:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to delete notification",
        });
      }
    });

    socket.on(SOCKET_EVENTS.NOTIFICATIONS_CLEAR, async () => {
      console.log("[Socket] Clearing all notifications");
      // Clear all notifications from database
      try {
        await notificationModel.clearNotifications(socket.data.user);
      } catch (error) {
        console.error("[Socket] Error clearing notifications:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to clear notifications",
        });
      }
    });

    /* ____________________________ Chat Events ___________________________ */

    socket.on(SOCKET_EVENTS.CHATS_FETCH, async (callback) => {
      console.log(
        "[Socket] Fetching chat messages for user:",
        socket.data.user
      );
      // Fetch chat messages from database
      try {
        const chats = await chatModel.getChatMessages(socket.data.user);
        callback(chats);
      } catch (error) {
        console.error("[Socket] Error fetching chat messages:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to fetch chat messages",
        });
        callback([]);
      }
    });

    socket.on(SOCKET_EVENTS.MESSAGE_NEW, async (message: Message, callback) => {
      console.log("[Socket] New chat message:", message);
      // Save new message to database
      try {
        // Ensure message is from current user
        message.fromUserId = socket.data.user;
        const newMessage = await chatModel.saveMessage(message);
        // Confirm message saved
        callback(newMessage.message);
        // Emit new message to chat participants
        socket.emit(SOCKET_EVENTS.MESSAGE, newMessage.message);
        const otherUserId =
          newMessage.chat.user1Id === socket.data.user
            ? newMessage.chat.user2Id
            : newMessage.chat.user1Id;
        io.to(`user_${otherUserId}`).emit(
          SOCKET_EVENTS.MESSAGE,
          newMessage.message
        );
        addNotification(
          {
            type: NotificationType.MESSAGE,
            ui_variant: "info",
            content: { message: "You have a new message!" },
            toUserID: otherUserId,
            fromUserID: socket.data.user,
          },
          [otherUserId]
        );
      } catch (error) {
        console.error("[Socket] Error saving new message:", error);
        socket.emit(SOCKET_EVENTS.ERROR, {
          message: "Failed to save new message",
        });
        callback(error);
      }
    });

    socket.on(
      SOCKET_EVENTS.MESSAGES_READ,
      async (messageIds: number[], callback) => {
        // Mark message as read in database
        try {
          if (messageIds.length === 0) {
            return;
          }
          if (messageIds.some((id) => isNaN(id))) {
            throw new Error("Invalid message id");
          }
          const userId = socket.data.user;
          const updatedMessages = await chatModel.markMessagesAsRead(
            userId,
            messageIds
          );
          // console.log(
          //   `[Socket] Marked messages as read for ${userId}:`,
          //   updatedMessages
          // );
          console.log(`[Socket] Marked messages as read for ${userId}:`);
          callback(updatedMessages);
        } catch (error) {
          console.error("[Socket] Error marking message as read:", error);
          socket.emit(SOCKET_EVENTS.ERROR, {
            message: "Failed to mark message as read",
          });
        }
      }
    );

    socket.on(SOCKET_EVENTS.DISCONNECT, async () => {
      console.log("[Socket] Client disconnected:", socket.id);
      try {
        // Update last seen time in database
        await user.updateLastSeen(socket.data.user);
      } catch (error) {
        console.error("[Socket] Error updating last seen:", error);
      }
    });
  });

  /* ________________________________ Socket Adapter ________________________________ */

  // Monitor room joins
  io.of("/").adapter.on("join-room", (room, id) => {
    console.log(`[Socket] ${id} joined room: ${room}`);
  });

  return io;
};
