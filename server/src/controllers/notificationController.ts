import { NotificationInterface } from "@interfaces/notificationInterface";
import { notificationModel } from "@src/models/notificationModel";
import { io } from "@src/server";
import { NotificationPayload } from "@interfaces/notificationInterface";
import { SOCKET_EVENTS } from "@src/interfaces/socketEventsInterface";
import { Chat } from "@src/interfaces/chatInterface";

export const addNotification = async (
  notification: NotificationInterface,
  userIds: number[]
): Promise<void> => {
  // Save notification to database
  const notifs = await notificationModel.createNotification(
    notification,
    userIds
  );

  // Emit notification to users if any exist
  if (notifs.length > 0) {
    const notificationPayload: NotificationInterface = {
      id: 0,
      type: notification.type,
      ui_variant: notification.ui_variant,
      content: notification.content,
      toUserID: 0,
      fromUserID: notification.fromUserID,
      isRead: false,
      createdAt: new Date(),
    };

    for (const notif of notifs) {
      // Update payload with recipient specific data
      notificationPayload.id = notif.id;
      notificationPayload.toUserID = notif.toUserID;
      notificationPayload.isRead = notif.isRead;
      notificationPayload.createdAt = notif.createdAt;

      try {
        const userRoom = `user_${notif.toUserID}`;
        if (!io.sockets.adapter.rooms.has(userRoom)) {
          // User is offline
          console.log(
            `[Socket] User_${notif.toUserID} is offline, skipping notification`
          );
          continue;
        }
        console.log(io.sockets.adapter.rooms);
        const response = await io
          .to(userRoom)
          .timeout(5000)
          .emitWithAck(SOCKET_EVENTS.NOTIFICATION_NEW, notificationPayload);
        // Update notification status to sent
        if (response && notif.id) {
          const response = await notificationModel.updateNotification([
            notif.id,
          ]);
        }
      } catch (error) {
        console.log(
          `[Socket] Error emitting notification to user_${notif.toUserID}:`,
          error
        );
      }
    }
  }
  return;
};

export const addChatEvent = async (
  chat: Chat,
  userIds: number[]
): Promise<void> => {
  // Emit chat event to users
  const chatPayload: Chat = {
    id: chat.id,
    messages: chat.messages,
    user1Id: chat.user1Id,
    user2Id: chat.user2Id,
    createdAt: chat.createdAt,
    deletedBy: chat.deletedBy,
  };

  for (const userId of userIds) {
    try {
      const userRoom = `user_${userId}`;
      if (!io.sockets.adapter.rooms.has(userRoom)) {
        // User is offline
        console.log(`[Socket] User_${userId} is offline, skipping chat event`);
        continue;
      }
      console.log(io.sockets.adapter.rooms);
      // TODO: Emit chat event to user (here not working)
      io.to(userRoom).emit(SOCKET_EVENTS.CHATS_NEW, chatPayload);
      console.log(`[Socket] Emitting new chat to user_${userId}`);
    } catch (error) {
      console.log(
        `[Socket] Error emitting chat event to user_${userId}:`,
        error
      );
    }
  }
  return;
};

export const deleteChatEvent = async (chatId: number, userIds: number[]) => {
  // Emit chat event to users
  for (const userId of userIds) {
    try {
      const userRoom = `user_${userId}`;
      if (!io.sockets.adapter.rooms.has(userRoom)) {
        // User is offline
        console.log(`[Socket] User_${userId} is offline, skipping chat event`);
        continue;
      }
      console.log(io.sockets.adapter.rooms);
      io.to(userRoom).emit(SOCKET_EVENTS.CHATS_DELETE, chatId);
      console.log(`[Socket] Emitting delete chat to user_${userId}`);
    } catch (error) {
      console.log(
        `[Socket] Error emitting delete chat to user_${userId}:`,
        error
      );
    }
  }
  return;
};
