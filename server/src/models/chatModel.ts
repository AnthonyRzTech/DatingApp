import { QueryResult } from "pg";
import { pool } from "../settings";
import { Chat, Message } from "@interfaces/chatInterface";

interface DbChat {
  id: number;
  user1_id: number;
  user2_id: number;
  created_at: Date;
  deleted_by: number[] | null;
  messages?: DbMessage[];
}

interface DbMessage {
  id: number;
  content: string;
  chat_id: number;
  from_user_id: number;
  created_at: Date;
  is_read: boolean;
}

class ChatModel {
  async getChatMessages(userId: number): Promise<Chat[]> {
    try {
      const getChatQuery = {
        text: "SELECT * FROM chats WHERE user1_id = $1 OR user2_id = $1",
        values: [userId],
      };
      const resultChats: QueryResult<DbChat> = await pool.query(getChatQuery);
      // Parse the messages from the database
      const chats: DbChat[] = resultChats.rows;
      for (const chat of chats) {
        const getMessagesQuery = {
          text: "SELECT * FROM messages WHERE chat_id = $1",
          values: [chat.id],
        };
        const resultMessages: QueryResult<DbMessage> = await pool.query(
          getMessagesQuery
        );
        chat.messages = resultMessages.rows;
      }
      const parsedChats = chats.map((chat) => {
        return {
          id: chat.id,
          user1Id: chat.user1_id,
          user2Id: chat.user2_id,
          createdAt: chat.created_at,
          messages: chat.messages
            ? chat.messages.map((message: DbMessage) => {
                return {
                  id: message.id,
                  chatId: message.chat_id,
                  content: message.content,
                  fromUserId: message.from_user_id,
                  createdAt: message.created_at,
                  isRead: message.is_read,
                } as Message;
              })
            : [],
          deletedBy: chat.deleted_by,
        } as Chat;
      });
      return parsedChats;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async createChat(user1Id: number, user2Id: number): Promise<Chat> {
    try {
      const createChatQuery = {
        text: `INSERT INTO chats (user1_id, user2_id)
              SELECT $1, $2
              WHERE NOT EXISTS (
                SELECT 1 FROM chats WHERE (user1_id = $1 AND user2_id = $2) OR (user1_id = $2 AND user2_id = $1)
              )
              AND $1 <> $2
              RETURNING *`,
        values: [user1Id, user2Id],
      };
      const result: QueryResult<DbChat> = await pool.query(createChatQuery);
      const newChat: DbChat = result.rows[0];
      if (!newChat) {
        return {} as Chat; // No new chat created, return empty chat
      }
      console.log("[ChatModel] Created new chat:", newChat);
      return {
        id: newChat.id,
        user1Id: newChat.user1_id,
        user2Id: newChat.user2_id,
        createdAt: newChat.created_at,
        messages: [],
        deletedBy: newChat.deleted_by,
      } as Chat;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async deleteChat(userId: number, matchedUserId: number): Promise<Chat> {
    try {
      const deleteChatQuery = {
        text: `DELETE FROM chats
              WHERE (user1_id = $1 AND user2_id = $2) OR (user1_id = $2 AND user2_id = $1)
              RETURNING chats.id, chats.user1_id, chats.user2_id, chats.created_at, chats.deleted_by`,
        values: [userId, matchedUserId],
      };
      const res = await pool.query(deleteChatQuery);
      console.log(
        "[ChatModel] Deleted chat between users:",
        userId,
        matchedUserId
      );
      const deletedChat: DbChat = res.rows[0];
      if (!deletedChat) {
        return {} as Chat; // No chat deleted, return empty chat
      }
      return {
        id: deletedChat.id,
        user1Id: deletedChat.user1_id,
        user2Id: deletedChat.user2_id,
        createdAt: deletedChat.created_at,
        deletedBy: deletedChat.deleted_by,
      } as Chat;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async saveMessage(
    message: Message
  ): Promise<{ message: Message; chat: Chat }> {
    try {
      // Check if chat exists and user is part of the chat
      const checkChatQuery = {
        text: "SELECT * FROM chats WHERE id = $1",
        values: [message.chatId],
      };
      const resultChat: QueryResult<DbChat> = await pool.query(checkChatQuery);
      if (resultChat.rowCount === 0) {
        throw new Error("Chat not found");
      }
      const chat: DbChat = resultChat.rows[0];
      if (
        chat.user1_id !== message.fromUserId &&
        chat.user2_id !== message.fromUserId
      ) {
        throw new Error("User not part of chat");
      }
      // Save the message to the database
      const saveMessageQuery = {
        text: "INSERT INTO messages (content, chat_id, from_user_id) VALUES ($1, $2, $3) RETURNING *",
        values: [message.content, message.chatId, message.fromUserId],
      };
      const result: QueryResult<DbMessage> = await pool.query(saveMessageQuery);
      const newMessage: DbMessage = result.rows[0];
      console.log("[ChatModel] Saved new message:", newMessage);
      return {
        message: {
          id: newMessage.id,
          chatId: newMessage.chat_id,
          content: newMessage.content,
          fromUserId: newMessage.from_user_id,
          createdAt: newMessage.created_at,
          isRead: false,
        } as Message,
        chat: {
          id: chat.id,
          user1Id: chat.user1_id,
          user2Id: chat.user2_id,
          createdAt: chat.created_at,
          deletedBy: chat.deleted_by,
        } as Chat,
      };
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async markMessagesAsRead(
    userId: number,
    messageIds: number[]
  ): Promise<Message[]> {
    try {
      const markMessagesQuery = {
        text: `UPDATE messages m
          SET is_read = true
          FROM chats c
          WHERE m.chat_id = c.id
          AND m.id = ANY($1)
          AND (c.user1_id = $2 OR c.user2_id = $2)  -- Ensure user is part of chat
          AND m.from_user_id != $2                  -- Ensure user is not sender
          RETURNING m.*`,
        values: [messageIds, userId],
      };
      const result: QueryResult<DbMessage> = await pool.query(
        markMessagesQuery
      );
      const updatedMessages: DbMessage[] = result.rows;
      return updatedMessages.map((message) => {
        return {
          id: message.id,
          chatId: message.chat_id,
          content: message.content,
          fromUserId: message.from_user_id,
          createdAt: message.created_at,
          isRead: message.is_read,
        } as Message;
      });
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }
}

export const chatModel = new ChatModel();
