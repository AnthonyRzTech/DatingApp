import { pool } from "../settings";

class BlockedUserModel {
  async blockUser(userId: number, blockedUserId: number) {
    try {
      const query = {
        text: `
        INSERT INTO blocked (blocker_id, blocked_user_id)
        VALUES ($1, $2)
        ON CONFLICT (blocker_id, blocked_user_id) DO NOTHING
        RETURNING *
        `,
        values: [userId, blockedUserId],
      };
      const result = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async unblockUser(userId: number, blockedUserId: number) {
    try {
      const query = {
        text: `
            DELETE FROM blocked
            WHERE blocker_id = $1 AND blocked_user_id = $2
            RETURNING *
        `,
        values: [userId, blockedUserId],
      };
      const result = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getBlockedUsers(userId: number) {
    try {
      const query = {
        text: `
            SELECT * FROM blocked
            WHERE blocker_id = $1
        `,
        values: [userId],
      };
      const result = await pool.query(query);
      return result.rows;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }
}

export const blocked = new BlockedUserModel();
