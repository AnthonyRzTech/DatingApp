import { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "@utils/authContext";
import { enqueueSnackbar } from "notistack";
import {
  NotificationInterface,
  ChatInterface,
  Message,
  SOCKET_EVENTS,
} from "@utils/socket";

interface payloadInterface {
  chats: ChatInterface[];
  chatNewMessage: (message: Message) => void;
  messagesMarkAsRead: (messageIds: number[]) => void;
  notifications: NotificationInterface[];
  notifMarkAsRead: (id: number) => void;
  notifMarkAsUnread: (id: number) => void;
  notifDelete: (id: number) => void;
  notifClear: () => void;
}

// Create a context to store the payload
const PayloadContext = createContext<payloadInterface | undefined>(undefined);

// Create a consumer for the payload context
export const usePayload = () => {
  const context = useContext(PayloadContext);
  if (context === undefined) {
    throw new Error("usePayload must be used within a PayloadProvider");
  }
  return context;
};

// Create a provider for the payload context
export const PayloadProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [notifications, setNotifications] = useState<NotificationInterface[]>(
    []
  );
  const [chats, setChats] = useState<ChatInterface[]>([]);
  const {
    isAuthenticated: isAuth,
    isLoading: authIsLoading,
    socket,
  } = useAuth();

  useEffect(() => {
    if (
      authIsLoading ||
      isAuth === false ||
      socket === null ||
      socket === undefined
    ) {
      console.log("Socket: PayloadProvider pre auth", socket);
      return;
    }
    console.log("Socket: PayloadProvider post auth", socket);
    // ____________________________ NOTIFICATIONS ____________________________
    // Fetch notifications
    socket
      .timeout(5000)
      .emit(
        SOCKET_EVENTS.NOTIFICATIONS_FETCH,
        (err: any, data: NotificationInterface[]) => {
          if (err) {
            console.error("Socket: Notifications Fetch Error", err);
            setNotifications([]);
            return;
          }
          console.log("Socket: Notifications Fetch", data);
          setNotifications(data);
        }
      );

    // Notifications event listeners
    socket.on(
      SOCKET_EVENTS.NOTIFICATION_NEW,
      (payload: NotificationInterface, callback) => {
        console.log("Socket: Notification", payload);
        callback("Notification received");
        if (payload.id in notifications.map((n) => n.id)) {
          console.log("Socket: Notification already exists", payload);
          return;
        }
        enqueueSnackbar(payload.content.message, {
          variant: payload.ui_variant || "default",
        });
        setNotifications((prev) => [...prev, payload]);
      }
    );

    // _________________________________ CHAT ________________________________
    // Fetch chat messages
    socket
      .timeout(5000)
      .emit(SOCKET_EVENTS.CHATS_FETCH, (err: any, data: ChatInterface[]) => {
        if (err) {
          console.error("Socket: Chat Fetch Error", err);
          setChats([]);
          return;
        }
        console.log("Socket: Chat Fetch", data);
        setChats(data);
      });

    // New chat group
    socket.on(SOCKET_EVENTS.CHATS_NEW, (payload: ChatInterface) => {
      console.log("Socket: New Chat", payload);
      setChats((prev) => {
        if (prev.find((chat) => chat.id === payload.id)) {
          return prev;
        }
        return [...prev, payload];
      });
    });

    // Delete chat group
    socket.on(SOCKET_EVENTS.CHATS_DELETE, (id: number) => {
      console.log("Socket: Delete Chat", id);
      setChats((prev) => prev.filter((chat) => chat.id !== id));
    });

    // Chat event listeners
    socket.on(SOCKET_EVENTS.MESSAGE, (payload: Message) => {
      console.log("Socket: Message received", payload);
      setChats((prev) =>
        prev.map((chat) => {
          if (chat.id === payload.chatId) {
            if (!chat.messages.find((msg) => msg.id === payload.id)) {
              return {
                ...chat,
                messages: [...chat.messages, payload],
              };
            }
          }
          return chat;
        })
      );
    });
  }, [isAuth, authIsLoading, socket]);

  const chatNewMessage = (message: Message) => {
    if (!socket) return;
    console.log("Socket: Sending New Chat Message", message);
    socket
      .timeout(5000)
      .emit(SOCKET_EVENTS.MESSAGE_NEW, message, (err: any, response: any) => {
        if (err) {
          console.error("Socket: New Chat Message Error", err);
          return;
        }
        if (response) {
          console.log("Socket: New Chat Message Confirmed", response);
        }
      });
  };

  const messagesMarkAsRead = (messageIds: number[]) => {
    if (!socket) return;
    console.log("Socket: Marking Message as Read", messageIds);
    socket
      .timeout(5000)
      .emit(
        SOCKET_EVENTS.MESSAGES_READ,
        messageIds,
        (err: any, response: any) => {
          if (err) {
            console.error("Socket: Mark Messages as Read Error", err);
            return;
          }
          if (response) {
            console.log("Socket: Mark Messages as Read Confirmed", response);
            setChats((prev) =>
              prev.map((chat) => {
                return {
                  ...chat,
                  messages: chat.messages.map((msg) =>
                    messageIds.includes(msg.id) ? { ...msg, isRead: true } : msg
                  ),
                };
              })
            );
          }
        }
      );
  };

  const notifMarkAsRead = (id: number) => {
    if (!socket) return;
    socket.emit(SOCKET_EVENTS.NOTIFICATION_READ, id);
    setNotifications((prev) =>
      prev.map((notif) =>
        notif.id === id ? { ...notif, isRead: true } : notif
      )
    );
  };

  const notifMarkAsUnread = (id: number) => {
    if (!socket) return;
    socket.emit(SOCKET_EVENTS.NOTIFICATION_UNREAD, id);
    setNotifications((prev) =>
      prev.map((notif) =>
        notif.id === id ? { ...notif, isRead: false } : notif
      )
    );
  };

  const notifDelete = (id: number) => {
    if (!socket) return;
    socket.emit(SOCKET_EVENTS.NOTIFICATION_DELETE, id);
    setNotifications((prev) => prev.filter((notif) => notif.id !== id));
  };

  const notifClear = () => {
    if (!socket) return;
    socket.emit(SOCKET_EVENTS.NOTIFICATIONS_CLEAR);
    setNotifications([]);
  };

  return (
    <PayloadContext.Provider
      value={{
        chats,
        chatNewMessage,
        messagesMarkAsRead,
        notifications,
        notifMarkAsRead,
        notifMarkAsUnread,
        notifDelete,
        notifClear,
      }}
    >
      {children}
    </PayloadContext.Provider>
  );
};
