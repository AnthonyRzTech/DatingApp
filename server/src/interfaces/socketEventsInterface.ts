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
