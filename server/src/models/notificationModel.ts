import { QueryResult } from "pg";
import { pool } from "../settings";
import {
  NotificationInterface,
  NotificationStatus,
  NotificationType,
} from "@src/interfaces/notificationInterface";


class NotificationModel {
  async getNotifications(userId: number): Promise<NotificationInterface[]> {
    const query = {
      text: `SELECT nr.id, no.type, no.content, nr.to_user_id, nr.from_user_id, nr.status, nr.is_read, nr.read_at, nr.created_at 
          FROM notification_recipients AS nr 
          JOIN notification_objects AS no ON nr.notification_object_id = no.id
          WHERE nr.to_user_id = $1`,
      values: [userId],
    };
    const result = await pool.query(query);
    const resultNotifications: NotificationInterface[] = result.rows.map(
      (row) => {
        return {
          id: row.id,
          type: row.type as NotificationType,
          content: row.content,
          toUserID: row.to_user_id,
          fromUserID: row.from_user_id,
          status: row.status as NotificationStatus,
          isRead: row.is_read as boolean,
          readAt: row.read_at as Date,
          createdAt: row.created_at as Date,
        };
      }
    );
    return resultNotifications;
  }

  async createNotification(
    notification: NotificationInterface,
    userIds: number[]
  ): Promise<NotificationInterface[]> {
    const query = {
      text: `
        WITH notif_object AS (
          INSERT INTO notification_objects (type, content)
          VALUES ($1, $2)
          RETURNING id
        )
        INSERT INTO notification_recipients (to_user_id, from_user_id, notification_object_id, status)
        SELECT unnest($3::integer[]), $4, id, 'PENDING'
        FROM notif_object
        RETURNING notification_recipients.*
      `,
      values: [
        notification.type,
        JSON.stringify(notification.content),
        userIds,
        notification.fromUserID,
      ],
    };
    const result = await pool.query(query);
    const resultNotifications: NotificationInterface[] = result.rows.map(
      (row) => {
        return {
          id: row.id,
          type: notification.type as NotificationType,
          ui_variant: notification.ui_variant,
          content: notification.content,
          toUserID: row.to_user_id,
          fromUserID: row.from_user_id,
          status: row.status as NotificationStatus,
          isRead: row.is_read as boolean,
          readAt: row.read_at as Date,
          createdAt: row.created_at as Date,
        };
      }
    );
    return resultNotifications;
  }

  async markNotificationAsRead(notificationId: number[]): Promise<any[]> {
    const query = {
      text: `UPDATE notification_recipients
          SET status = 'SENT'::notification_status, is_read = true, read_at = NOW()
          WHERE id = ANY($1::integer[])
          RETURNING notification_recipients.*`,
      values: [notificationId],
    };
    const result = await pool.query(query);
    return result.rows;
  }

  async markNotificationAsUnread(notificationId: number[]): Promise<any[]> {
    const query = {
      text: `UPDATE notification_recipients
          SET status = 'SENT'::notification_status, is_read = false, read_at = NULL
          WHERE id = ANY($1::integer[])
          RETURNING notification_recipients.*`,
      values: [notificationId],
    };
    const result = await pool.query(query);
    return result.rows;
  }
  async updateNotification(notificationId: number[]): Promise<any[]> {
    const query = {
      text: `UPDATE notification_recipients
          SET status = 'SENT'::notification_status
          WHERE id = ANY($1::integer[])
          RETURNING notification_recipients.*`,
      values: [notificationId],
    };
    const result = await pool.query(query);
    return result.rows;
  }

  async deleteNotification(notificationId: number): Promise<any[]> {
    // check if the notification object is connected to multiple recipients
    const checkQuery = {
      text: `SELECT COUNT(*) FROM notification_recipients
          WHERE notification_object_id = (SELECT notification_object_id FROM notification_recipients WHERE id = $1)`,
      values: [notificationId],
    };
    const checkResult = await pool.query(checkQuery);
    const count = checkResult.rows[0].count;

    if (count > 1) {
      // If the notification object is connected to multiple recipients, only delete the recipient
      const query = {
        text: `DELETE FROM notification_recipients
            WHERE id = $1
            RETURNING *`,
        values: [notificationId],
      };
      const result = await pool.query(query);
      return result.rows;
    } else {
      // If the notification object is connected to only one recipient, delete the recipient and the notification object
      const query = {
        text: `SELECT notification_object_id FROM notification_recipients WHERE id = $1`,
        values: [notificationId],
      };
      const result = await pool.query(query);
      const objectId = result.rows[0].notification_object_id;

      // Delete the recipient
      const deleteRecipientQuery = {
        text: `DELETE FROM notification_recipients
            WHERE id = $1
            RETURNING *`,
        values: [notificationId],
      };
      const deleteRecipientResult = await pool.query(deleteRecipientQuery);

      // Delete the notification object
      const deleteObjectQuery = {
        text: `DELETE FROM notification_objects WHERE id = $1`,
        values: [objectId],
      };
      const deleteObjectResult = await pool.query(deleteObjectQuery);

      return deleteRecipientResult.rows;
    }
  }

  async clearNotifications(userId: number): Promise<any[]> {
    // Recover all notification recipient ids
    const notificationRecipientIdsQuery = {
      text: `SELECT id FROM notification_recipients
          WHERE to_user_id = $1`,
      values: [userId],
    };
    const result = await pool.query(notificationRecipientIdsQuery);
    const notificationIds = result.rows.map((row) => row.id);

    // Delete all notification recipients
    let results: QueryResult[] = [];
    for (const id of notificationIds) {
      const result = await this.deleteNotification(id);
      results.concat(result);
    }
    return results;
  }
}

export const notificationModel = new NotificationModel();
